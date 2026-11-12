namespace talaria_links_api.Common
{
    public static class LikePattern
    {
        public const string EscapeChar = "\\";

        public static string Contains(string value)
        {
            var escaped = value.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            return $"%{escaped}%";
        }
    }
}
