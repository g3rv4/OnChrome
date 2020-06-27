namespace OnChrome.Core
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);

        public static bool HasValue(this string? str) => !str.IsNullOrEmpty();
    }
}
