using System;

namespace Poker.Lib.Util
{
    public static class Extensions
    {
        public static byte[] Base64ToByteArray(this string s)
        {
            return Convert.FromBase64String(s);
        }
    }
}
