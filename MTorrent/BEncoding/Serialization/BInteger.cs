// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Diagnostics;
using System.Numerics;

namespace MTorrent.BEncoding.Serialization
{
    [DebuggerDisplay("{Value}")]
    public class BInteger : IBObject
    {
        public int SpanStart { get; set; }
        public int SpanEnd { get; set; }

        private readonly long _longValue;

        public BigInteger Value { get; }

        public BInteger(long value)
        {
            Value = _longValue = value;
        }

        public BInteger(BigInteger value)
        {
            Value = value;
            if (Value >= long.MinValue && Value <= long.MaxValue)
                _longValue = (long)Value;
        }

        public bool TryAs(out int value)
        {
            if ((_longValue == 0 && !Value.IsZero) || _longValue < int.MinValue || _longValue > int.MaxValue)
            {
                value = 0;
                return false;
            }
            else
            {
                value = (int)_longValue;
                return true;
            }
        }

        public bool TryAs(out long value)
        {
            if (_longValue == 0 && !Value.IsZero)
            {
                value = 0;
                return false;
            }
            else
            {
                value = _longValue;
                return true;
            }
        }

        public bool TryAs(out uint value)
        {
            if (_longValue == 0 && !Value.IsZero)
            {
                value = 0;
                return false;
            }
            else
            {
                value = (uint)_longValue;
                return true;
            }
        }

        public bool TryAs(out ulong value)
        {
            if (_longValue >= 0 && Value.Sign >= 0)
            {
                if (_longValue != 0 || Value.IsZero)
                {
                    value = (ulong)_longValue;
                    return true;
                }
                if (Value <= ulong.MaxValue)
                {
                    value = (ulong)Value;
                    return true;
                }
            }

            value = 0;
            return false;
        }
    }
}
