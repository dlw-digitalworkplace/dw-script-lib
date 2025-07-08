using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System.Text;

namespace Services.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public List<string> GetFileContent(string containerName, string filePathWithExtension)
        {
            var blobClient = GetBlobClient(containerName, filePathWithExtension);

            if (!blobClient.Exists())
            {
                return new List<string>();
            }

            var downloadInfo = blobClient.DownloadContent();
            var content = Encoding.UTF8.GetString(downloadInfo.Value.Content.ToArray());

            return content.Split(Environment.NewLine).ToList();
        }

        private BlobClient GetBlobClient(string containerName, string filePathWithExtension)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            container.CreateIfNotExists();
            return container.GetBlobClient(filePathWithExtension);
        }
    }
}
