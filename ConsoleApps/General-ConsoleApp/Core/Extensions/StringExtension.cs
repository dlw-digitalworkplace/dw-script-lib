namespace Core.Extensions
{
    public static class StringExtension
    {
        public static int GetIntConfigValue(string? configValue)
        {
            var config =  string.IsNullOrWhiteSpace(configValue) ? 0 : int.Parse(configValue);
            return config;
        }

    }
}
