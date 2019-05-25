using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Torrent.BEncoding.Serialization
{
    public static partial class BEncodingSerializer
    {
        public static bool TryParse(string data, out IBObject bObject, bool strictDictionaryOrder = true)
            => TryParse(BEncodingHelpers.UTF8.GetBytes(data), out bObject, strictDictionaryOrder);

        public static bool TryParse(ReadOnlySpan<byte> data, out IBObject bObject, bool strictDictionaryOrder = true)
        {
            var reader = new Utf8BEncodingReader(data);
            List<IBObject> stack = null;
            bObject = null;
            BString key = null;

            while (reader.Read())
            {
                IBObject value;
                switch (reader.TokenType)
                {
                    case BEncodingTokenType.String:
                        value = new BString(reader.ValueSpan);
                        break;

                    case BEncodingTokenType.Integer:
                        if (!reader.TryGet(out BigInteger integer))
                            return false;

                        value = new BInteger(integer);
                        break;

                    case BEncodingTokenType.End:
                        if (stack is null || !(key is null))
                            return false;

                        stack[stack.Count - 1].SpanEnd = reader.Consumed;

                        if (stack.Count == 1)
                            return reader.IsEmpty;

                        Debug.Assert(stack.Count != 0);

                        stack.RemoveAt(stack.Count - 1);
                        continue;

                    default:
                        Debug.Assert(reader.TokenType == BEncodingTokenType.StartDictionary || reader.TokenType == BEncodingTokenType.StartList);
                        value = reader.TokenType == BEncodingTokenType.StartDictionary
                            ? (IBObject)new BDictionary()
                            : (IBObject)new BList();

                        if (stack is null)
                        {
                            Debug.Assert(key is null);
                            stack = new List<IBObject>(4);
                            bObject = value;
                            stack.Add(bObject);
                            continue;
                        }
                        else
                        {
                            if (stack.Count == BEncodingConstants.DepthMax)
                                return false;
                            break;
                        }
                }

                if (stack is null)
                {
                    Debug.Assert(bObject is null && key is null);
                    Debug.Assert(reader.TokenType != BEncodingTokenType.StartDictionary);
                    Debug.Assert(reader.TokenType != BEncodingTokenType.StartList);
                    bObject = value;
                    return reader.IsEmpty;
                }

                if (stack.Count == 0)
                    return false;

                var top = stack[^1];

                if (top is BDictionary dictionary)
                {
                    if (key is null)
                    {
                        if (value is BString keyValue)
                        {
                            key = keyValue;
                            if (strictDictionaryOrder && dictionary.Count != 0)
                            {
                                if (key.Binary.AsSpan().SequenceCompareTo(dictionary[^1].Key.Binary.AsSpan()) < 0)
                                    return false;
                            }
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        dictionary.Add(new KeyValuePair<BString, IBObject>(key, value));
                        key = null;
                    }
                }
                else
                {
                    Debug.Assert(top is BList);
                    Debug.Assert(key is null);
                    ((BList)top).Add(value);
                }

                if (reader.TokenType == BEncodingTokenType.StartDictionary ||
                    reader.TokenType == BEncodingTokenType.StartList)
                {
                    value.SpanStart = reader.Consumed - 1;
                    stack.Add(value);
                }
            }

            return false;
        }

        public static int Encode(IBObject bObject, Stream destination)
        {
            if (bObject is BDictionary dictionary) return Encode(dictionary, destination);
            else if (bObject is BList list) return Encode(list, destination);
            else if (bObject is BString bString) return Encode(bString, destination);
            else return Encode(bObject as BInteger, destination);
        }
        public static int Encode(IBObject bObject, IBufferWriter<byte> bufferWriter)
        {
            if (bObject is BDictionary dictionary) return Encode(dictionary, bufferWriter);
            else if (bObject is BList list) return Encode(list, bufferWriter);
            else if (bObject is BString bString) return Encode(bString, bufferWriter);
            else return Encode(bObject as BInteger, bufferWriter);
        }

        public static int Encode(BDictionary dictionary, Stream destination)
            => EncodeContainerCore(dictionary, new Utf8BEncodingWriter(destination));

        public static int Encode(BDictionary dictionary, IBufferWriter<byte> bufferWriter)
            => EncodeContainerCore(dictionary, new Utf8BEncodingWriter(bufferWriter));

        public static int Encode(BList list, Stream destination)
            => EncodeContainerCore(list, new Utf8BEncodingWriter(destination));

        public static int Encode(BList list, IBufferWriter<byte> bufferWriter)
            => EncodeContainerCore(list, new Utf8BEncodingWriter(bufferWriter));

        private static int EncodeContainerCore(IBObject initialContainer, Utf8BEncodingWriter writer)
        {
            List<IBObject> containerStack = null;
            Span<int> indexStack = stackalloc int[BEncodingConstants.DepthMax];
            int depth = 1;

            IBObject container = initialContainer;
            if (container is BDictionary) writer.WriteDictionaryStart();
            else writer.WriteListStart();

            int i = 0;
            do
            {
                if ((container as IList).Count == i)
                {
                    writer.WriteEnd();
                    depth--;
                    if (depth != 0)
                    {
                        Debug.Assert(containerStack != null);
                        Debug.Assert(depth == containerStack.Count);
                        container = containerStack[depth - 1];
                        containerStack.RemoveAt(depth - 1);
                        i = indexStack[depth];
                        indexStack[depth] = 0;
                    }
                    continue;
                }

                IBObject value;
                if (container is BDictionary dict)
                {
                    KeyValuePair<BString, IBObject> pair = dict[i];
                    writer.Write(pair.Key.Binary);
                    value = pair.Value;
                }
                else
                {
                    value = (container as BList)[i];
                }
                i++;

                if (value is BString bstring)
                {
                    writer.Write(bstring.Binary);
                    continue;
                }
                else if (value is BInteger integer)
                {
                    writer.Write(integer.Value);
                    continue;
                }

                if (depth == BEncodingConstants.DepthMax)
                    throw new Exception("Too much depth");

                if (containerStack is null) containerStack = new List<IBObject>(4);
                containerStack.Add(container);
                indexStack[depth++] = i;
                i = 0;
                container = value;
                if (container is BDictionary) writer.WriteDictionaryStart();
                else writer.WriteListStart();
            }
            while (depth != 0);

            writer.Flush();

            return writer.BytesCommitted;
        }

        public static int Encode(BInteger integer, Stream destination)
        {
            var writer = new Utf8BEncodingWriter(destination);
            writer.Write(integer.Value);
            writer.Flush();
            return writer.BytesCommitted;
        }

        public static int Encode(BInteger integer, IBufferWriter<byte> bufferWriter)
        {
            var writer = new Utf8BEncodingWriter(bufferWriter);
            writer.Write(integer.Value);
            writer.Flush();
            return writer.BytesCommitted;
        }

        public static int Encode(BString value, Stream destination)
        {
            var writer = new Utf8BEncodingWriter(destination);
            writer.Write(value.Binary);
            writer.Flush();
            return writer.BytesCommitted;
        }

        public static int Encode(BString value, IBufferWriter<byte> bufferWriter)
        {
            var writer = new Utf8BEncodingWriter(bufferWriter);
            writer.Write(value.Binary);
            writer.Flush();
            return writer.BytesCommitted;
        }
    }
}
