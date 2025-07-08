using Core;
using Core.Enums;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using Services.Interfaces.SharePoint;

namespace Services.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILocalFileService _localFileService;
        private readonly ISharePointService _sharePointService;
        private readonly IBlobService? _blobService;
        private readonly IConfiguration _configuration;

        public ConfigurationService(ILocalFileService localFileService, ISharePointService sharePointService, IConfiguration configuration)
        {
            _localFileService = localFileService;
            _sharePointService = sharePointService;
            _configuration = configuration;
        }

        public ConfigurationService(ILocalFileService localFileService, ISharePointService sharePointService, IBlobService blobService, IConfiguration configuration)
        {
            _localFileService = localFileService;
            _sharePointService = sharePointService;
            _blobService = blobService;
            _configuration = configuration;
        }

        public async Task<List<string>> GetSharePointSiteURLs(DataSource dataSource)
        {
            var urls = new List<string>();
            switch (dataSource)
            {
                case DataSource.AppSettings:
                    urls = GetSharePointSiteURLsFromAppSettings();
                    break;
                case DataSource.Platform:
                    urls = await GetSharePointSiteURLsFromPlatform();
                    break;
                case DataSource.PlatformWithPrefixFilter:
                    urls = await GetSharePointSiteURLsFromPlatformWithPrefixFilter();
                    break;
                case DataSource.LocalFile:
                    urls = GetSharePointSiteURLsFromLocalFile();
                    break;
                case DataSource.AzureBlob:
                    urls = GetSharePointSiteURLsFromAzureBlob();
                    break;
            }

            return urls;
        }

        private List<string> GetSharePointSiteURLsFromAppSettings()
        {
            var urls = new List<string>();

            var urlsConfig = _configuration[Globals.General.DataSource.Value];
            if (null != urlsConfig)
            {
                urls = urlsConfig.Split(';').ToList();
            }

            return urls;
        }

        private async Task<List<string>> GetSharePointSiteURLsFromPlatform()
        {
            var urls = new List<string>();

            using (var context = await _sharePointService.CreateClientContextAsync($"https://{_configuration[Globals.Tenant.Name]}-admin.sharepoint.com/"))
            {
                var allSiteCollections = await _sharePointService.GetAllSiteCollectionsAsync(context);
                urls = allSiteCollections.Select(sc => sc.Url).ToList();
            }

            return urls;
        }

        private async Task<List<string>> GetSharePointSiteURLsFromPlatformWithPrefixFilter()
        {
            var urls = new List<string>();

            using (var context = await _sharePointService.CreateClientContextAsync($"https://{_configuration[Globals.Tenant.Name]}-admin.sharepoint.com/"))
            {
                var filteredSiteCollections = await _sharePointService.GetAllSiteCollectionURLsByPrefixAsync(context);
                urls = filteredSiteCollections.ToList();
            }

            return urls;
        }

        private List<string> GetSharePointSiteURLsFromLocalFile()
        {
            var urls = new List<string>();

            var filePath = _configuration[Globals.General.DataSource.Value];
            if (null != filePath)
            {
                urls = _localFileService.ReadLinesFromFile(filePath);
            }

            return urls;
        }

        private List<string> GetSharePointSiteURLsFromAzureBlob()
        {
            var urls = new List<string>();
            var container = _configuration[Globals.AzureBlob.Container];
            var filePathWithExtension = _configuration[Globals.AzureBlob.FilePathWithExtension];

            if (null == container || null == filePathWithExtension)
            {
                return urls;
            }

            urls = _blobService.GetFileContent(container, filePathWithExtension);

            return urls;
        }
    }
}
