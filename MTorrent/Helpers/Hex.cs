using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Torrent.Helpers
{
    internal static class Hex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetValue(char c)
            => (c >= '0' && c <= '9')
                ? c - '0'
                : (c >= 'A' && c <= 'F')
                    ? c - ('A' - 10)
                    : (c >= 'a' && c <= 'f')
                        ? c - ('a' - 10)
                        : -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetChar(int value)
            => (char)(value < 10
                ? value + '0'
                : value + 'a' - 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetCharUpperCase(int value)
            => (char)(value < 10
                ? value + '0'
                : value + 'A' - 10);

        public static bool TryParse(ReadOnlySpan<char> hex, out byte[] bytes)
        {
            bytes = null;

            if ((hex.Length & 1) == 1)
                return false;

            bytes = new byte[hex.Length >> 1];

            for (int i = 0; i < bytes.Length; i++)
            {
                int hi = GetValue(hex[i * 2]);
                int lo = GetValue(hex[i * 2 + 1]);

                if (hi == -1 || lo == -1)
                    return false;

                bytes[i] = (byte)((hi << 4) | lo);
            }

            return true;
        }

        public static void Encode(ReadOnlySpan<byte> data, StringBuilder sb)
        {
            Span<char> hex = stackalloc char[128];

            while (data.Length > 0)
            {
                int size = Math.Min(64, data.Length);
                ReadOnlySpan<byte> chunk = data.Slice(0, size);
                data = data.Slice(size);

                for (int i = 0; i < chunk.Length; i++)
                {
                    hex[i * 2] = GetChar(chunk[i] >> 4);
                    hex[i * 2 + 1] = GetChar(chunk[i] & 0xF);
                }

                sb.Append(size == 64 ? hex : hex.Slice(0, 2 * size));
            }
        }
        public static void EncodeUpperCase(ReadOnlySpan<byte> data, StringBuilder sb)
        {
            Span<char> hex = stackalloc char[128];

            while (data.Length > 0)
            {
                int size = Math.Min(64, data.Length);
                ReadOnlySpan<byte> chunk = data.Slice(0, size);
                data = data.Slice(size);

                for (int i = 0; i < chunk.Length; i++)
                {
                    hex[i * 2] = GetCharUpperCase(chunk[i] >> 4);
                    hex[i * 2 + 1] = GetCharUpperCase(chunk[i] & 0xF);
                }

                sb.Append(size == 64 ? hex : hex.Slice(0, 2 * size));
            }
        }

        public static string Encode(ReadOnlySpan<byte> data)
        {
            char[] hex = new char[data.Length * 2];

            for (int i = 0; i < data.Length; i++)
            {
                hex[i * 2] = GetChar(data[i] >> 4);
                hex[i * 2 + 1] = GetChar(data[i] & 0xF);
            }

            return new string(hex);
        }

        public static string EncodeUpperCase(ReadOnlySpan<byte> data)
        {
            char[] hex = new char[data.Length * 2];

            for (int i = 0; i < data.Length; i++)
            {
                hex[i * 2] = GetCharUpperCase(data[i] >> 4);
                hex[i * 2 + 1] = GetCharUpperCase(data[i] & 0xF);
            }

            return new string(hex);
        }
    }
}
