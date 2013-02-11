namespace Videre.Core.Extensions
{
    public static class StringExtensions
    {
        public static string CoalesceString(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}