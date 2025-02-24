namespace Core.Extensions
{
    public static class StringExtension
    {
        public static int GetIntConfigValue(this string? configValue)
        {
            var config = int.TryParse(configValue, out int parsedValue) ? parsedValue : 0;
            return config;
        }

    }
}
