using System;
using System.Diagnostics;
using System.Text;

namespace Torrent.BEncoding
{
    internal static class BEncodingHelpers
    {
        public static readonly ASCIIEncoding ASCII = new ASCIIEncoding();
        public static readonly UTF8Encoding UTF8 = new UTF8Encoding(false, throwOnInvalidBytes: true);

        public static bool IsDigit(byte value) => (uint)(value - '0') <= '9' - '0';

        public static ulong Pow10(int e)
        {
            Debug.Assert(e < 19);
            return e < 10
                ? (ulong)Math.Pow(10, e)
                : (ulong)Math.Pow(10, e - 9) * 1_000_000_000u;
        }

        public static int DigitCount(int n)
            => // ToDo - benchmark
            n < 100
                ? (n < 10 ? 1 : 2)
                : n < 10000
                    ? (n < 1000 ? 3 : 4)
                    : n < 1000000
                        ? (n < 100000 ? 5 : 6)
                        : (int)Math.Log10(n) + 1;
    }
}
