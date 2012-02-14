using System;

namespace KhanProxy.Services
{
    public static class StringExtensions
    {
        public static Uri AsUri(this string value)
        {
            return AsUri(value, UriKind.RelativeOrAbsolute);
        }

        public static Uri AsUri(this string value, UriKind kind)
        {
            return new Uri(value, kind);
        }
    }
}