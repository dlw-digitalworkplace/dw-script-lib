using Microsoft.Extensions.Configuration;

public class Settings
{
    public GraphSettings? Graph { get; set; }
    public ItemPermissions? Item { get; set; }

    public static Settings LoadSettings()
    {
        // Load settings
        IConfiguration config = new ConfigurationBuilder()
            // appsettings.json is required
            .AddJsonFile("appsettings.json", optional: false)
            // appsettings.Development.json" is optional, values override appsettings.json
            .AddJsonFile($"appsettings.Development.json", optional: true)
            // User secrets are optional, values override both JSON files
            .AddUserSecrets<Program>()
            .Build();

        return config.GetRequiredSection("settings").Get<Settings>() ??
            throw new Exception("Could not load app settings. See README for configuration instructions.");
    }

    public class GraphSettings
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
    }

    public class ItemPermissions
    {
        public string? DocLibName { get; set; }
        public string? DriveItemId { get; set; }
        public string? SiteId { get; set; }
        public string? SiteName { get; set; }
    }
}