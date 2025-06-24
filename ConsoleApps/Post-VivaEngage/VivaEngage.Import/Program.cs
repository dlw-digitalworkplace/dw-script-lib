using System.Net;
using System.Net.Http.Headers;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace VivaEngageImport
{
    internal class Program
    {
        // See accompanying readMe for config Entra ID app & setup access token

        // June 2025, no throttling was encountered, so no retry logic is implemented. Delegated access token was used (token does expire after 1 hour).

        // Blob posts:
        // https://techcommunity.microsoft.com/blog/viva_engage_blog/calling-legacy-viva-engage-yammer-apis-via-an-entra-application/4412337
        // https://techcommunity.microsoft.com/blog/viva_engage_blog/yammer-api-with-aad-tokens-postman-collection/857923

        // API reference
        // https://learn.microsoft.com/en-us/rest/api/yammer/messages-json-post
        // https://learn.microsoft.com/en-us/rest/api/yammer/upload-files-into-yammer-groups

        // Useful SO post
        // https://stackoverflow.com/questions/66062985/how-to-upload-files-on-yammer-api-via-the-azure-upload-small-file-api/66062986#66062986

        // 
        // https://stackoverflow.com/questions/76383947/how-to-use-yammer-api-using-aad-authentication-method


        // secrets.json example:

        /*
        {
            "accessToken": "eyJ0*****m1mtg",
            "groupId": "", // Note: you can base64 decode the url in Viva Engage to get the group id. Sample id: 62538489857
            "networkId": "", // Note: you can get the network id from the Yammer API. Sample id: 4239127042
            "sitename": "archiefworkplace", // site url name in SharePoint site behind the VE community M365 group, where the media files are hosted
            "storage": "DefaultEndpointsProtocol=https;AccountName=custprodsa;AccountKey=****", // UseDevelopmentStorage=true for local development with Azurite
            "tenantName": "" // your tenant name, used in the SharePoint url
        } 
        */

        static readonly HttpClient HttpClient = new HttpClient();

        private const string YammerUrl = "https://www.yammer.com/api/v1/";
        private const string YammerFileUploadUrl = "https://filesng.yammer.com/v4/uploadSmallFile";

        private static BlobContainerClient BlobContainerClient;
        private static TableClient TableClient;

        private static string Sitename;
        private static string TenantName;
        private static string YammerGroupId;
        private static string YammerNetworkId;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<Program>();

            var configuration = builder.Build();
            var accessToken = configuration["accessToken"]; // Get from Postman, via interactive login. Valid for 60 minutes.
            var connectionString = configuration["storage"];

            Sitename = configuration["sitename"];
            YammerGroupId = configuration["groupId"];
            YammerNetworkId = configuration["networkId"];
            TenantName = configuration["tenantName"];

            var tableServiceClient = new TableServiceClient(connectionString);
            TableClient = tableServiceClient.GetTableClient("EngagePosts");

            var blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient = blobServiceClient.GetBlobContainerClient("metamigrationmedia");

            HttpClient.BaseAddress = new Uri(YammerUrl);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var posts = await GetOrderedPostsToMigrate();

            var index = 0;

            foreach (var p in posts)
            {
                index++;
                Console.Write($"{index}/{posts.Count} {p.RowKey}");

                var rowKey = p.RowKey;
                var post = (await TableClient.GetEntityAsync<EngagePost>("Engage", rowKey)).Value;

                if (string.IsNullOrWhiteSpace(post.Message + post.PhotoLocations + post.VideoLocations))
                {
                    post.Status = "Skipped";
                    post.MigrationComment = "No message nor media";
                    await TableClient.UpdateEntityAsync(post, ETag.All);

                    Console.WriteLine($" | marked as 'Skipped' (no message nor media)");

                    continue;
                }

                await ProcessPost(post);
            }
        }

        static async Task<List<EngagePost>> GetOrderedPostsToMigrate(string? next = null)
        {
            var pageable = TableClient.QueryAsync<EngagePost>(e => (e.Status == "To migrate" || e.Status == "Error"), 500, new List<string> { "RowKey", "Created" });

            var posts = new List<EngagePost>();
            await foreach (var item in pageable)
            {
                var engagePost = new EngagePost();
                engagePost.RowKey = item.RowKey;
                engagePost.Created = item.Created;
                posts.Add(engagePost);
            }

            posts = posts.OrderBy(e => e.Created).ToList();

            Console.WriteLine($"Creating {posts.Count} posts");

            return posts;
        }

        static async Task GetUploadedFiles()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"uploaded_files.json");
            var response = await HttpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
        }

        static async Task<bool> PostMessage(EngagePost post)
        {
            var imageAttachments = new List<string>();
            var videoAttachments = new List<string>();
            var largeImageUrls = new List<string>();
            var largeVideoUrls = new List<string>();

            if (!string.IsNullOrWhiteSpace(post.PhotoLocations))
            {
                var mediaType = "image";
                var (imageUploadSuccess, imageAttachmentsInner, largeImageUrlsInner) = await UploadAttachments(post, mediaType);

                if (!imageUploadSuccess)
                {
                    return false;
                }

                imageAttachments.AddRange(imageAttachmentsInner);
                largeImageUrls.AddRange(largeImageUrlsInner);
            }

            if (!string.IsNullOrWhiteSpace(post.VideoLocations))
            {
                var mediaType = "video";
                var (videoUploadSuccess, videoAttachmentsInner, largeVideoUrlsInner) = await UploadAttachments(post, mediaType);

                if (!videoUploadSuccess)
                {
                    return false;
                }

                videoAttachments.AddRange(videoAttachmentsInner);
                largeVideoUrls.AddRange(largeVideoUrlsInner);
            }

            Console.Write(" | posting...");

            var mediaUrls = string.Join("\n", largeImageUrls.Concat(largeVideoUrls).ToList());
            var mediaUrlsMessage = string.IsNullOrWhiteSpace(mediaUrls) ? "" : $"\n\nMedia:\n{mediaUrls}";
            var message = string.IsNullOrWhiteSpace(post.Message) ? "" : $"\n\n{post.Message}";
            var body = $"Door {post.UserName} op {post.Created:d/M/yyyy} in '{post.GroupName}'.{mediaUrlsMessage}{message}";

            var collection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("group_id", YammerGroupId),
                new KeyValuePair<string, string>("body", body),
            };

            foreach (var imageFileId in imageAttachments)
            {
                collection.Add(new KeyValuePair<string, string>("attached_objects[]", $"uploaded_file:{imageFileId}"));
            }

            foreach (var videoFileId in videoAttachments)
            {
                collection.Add(new KeyValuePair<string, string>("attached_objects[]", $"uploaded_file:{videoFileId}"));
            }

            var formContent = new FormUrlEncodedContent(collection);
            var request = new HttpRequestMessage(HttpMethod.Post, $"messages.json");
            request.Content = formContent;
            var response = await HttpClient.SendAsync(request);

            var statusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                Console.Write(" done!");
                return true;
            }
            else
            {
                Console.WriteLine("");

                //if (statusCode == HttpStatusCode.TooManyRequests)
                //{
                // handle throttling if needed
                //}

                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"\n\nEXPIRED access token for {post.RowKey}, exiting");

                    post.Status = "Error";
                    post.MigrationComment = $"Access token expired";
                    await TableClient.UpdateEntityAsync(post, ETag.All);

                    Console.ReadLine();
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"\n\nERROR: {statusCode} for {post.RowKey}");
                    post.Status = "Error";
                    post.MigrationComment = $"Error {response.StatusCode}: {content}";
                    await TableClient.UpdateEntityAsync(post, ETag.All);

                    Console.ReadLine();
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }

            return false;
        }

        static async Task ProcessPost(EngagePost post)
        {
            var success = await PostMessage(post);

            if (success)
            {
                post.Status = "Migrated";
                post.MigrationComment = $"Migrated via script to group {YammerGroupId}";
                await TableClient.UpdateEntityAsync(post, ETag.All);

                Console.WriteLine($" | marked as 'Migrated'");
            }
            else
            {
                post.Status = "Error";
                post.MigrationComment = $"Error while posting message";
                await TableClient.UpdateEntityAsync(post, ETag.All);

                Console.WriteLine($" | marked {post.RowKey} as 'Error'");
            }
        }

        static async Task TestMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"messages/my_feed.json");
            var response = await HttpClient.SendAsync(request);
        }

        static async Task<(bool, bool, string?)> UploadFile(EngagePost post, string filename, string rowKey)
        {
            var (uploadYammerSuccess, fileId) = await UploadFileToYammer(post, filename, rowKey);

            if (uploadYammerSuccess)
            {
                return (true, false, fileId);
            }

            var (uploadSharePointSuccess, fileUrl) = await UploadFileToSharePoint(filename);

            if (uploadSharePointSuccess)
            {
                return (false, true, fileUrl);
            }

            return (false, false, null);
        }

        static async Task<(bool, List<string>, List<string>)> UploadAttachments(EngagePost post, string mediaType)
        {
            var mediaAttachments = new List<string>();
            var largeMediaUrls = new List<string>();

            var mediaLocations = mediaType == "image" ? post.PhotoLocations?.Split('|') : post.VideoLocations?.Split('|');

            if (mediaLocations == null || !mediaLocations.Any())
            {
                return (true, mediaAttachments, largeMediaUrls);
            }

            var plural = mediaLocations.Length > 1 ? "s" : "";
            Console.Write($" | uploading {mediaLocations.Length} {mediaType}{plural}");

            foreach (var mediaLocation in mediaLocations.Take(100)) // note: max 100 images per post
            {
                var (uploadYammerSuccess, uploadSharePointSuccess, fileIdOrUrl) = await UploadFile(post, mediaLocation, post.RowKey!);

                if (uploadYammerSuccess)
                {
                    mediaAttachments.Add(fileIdOrUrl!);
                }
                else if (uploadSharePointSuccess)
                {
                    largeMediaUrls.Add(fileIdOrUrl!);
                }
                else
                {
                    Console.WriteLine($"\n\nERROR for {post.RowKey}: Could not upload {mediaType} {Path.GetFileName(mediaLocation)}");
                    return (false, mediaAttachments, largeMediaUrls);
                }
            }

            Console.Write($": done");

            return (true, mediaAttachments, largeMediaUrls);
        }

        static async Task<(bool, string?)> UploadFileToSharePoint(string filename)
        {
            // Here you would implement the logic to upload the file to SharePoint
            // and return the URL of the uploaded file.
            // Instead, the few uploads required will be done manually, the url can be inferred.

            await Task.CompletedTask;
            return (true, $"https://{TenantName}.sharepoint.com/sites/{Sitename}/Gedeelde%20documenten/Apps/Viva%20Engage/{Path.GetFileName(filename)}");
        }

        static async Task<(bool, string?)> UploadFileToYammer(EngagePost post, string filename, string rowKey)
        {
            // 4 MB is the limit for small files, larger files need to be uploaded via SharePoint API

            var fileWithoutPath = Path.GetFileName(filename);

            try
            {
                var blobClient = BlobContainerClient.GetBlobClient(fileWithoutPath);
                using var memStream = new MemoryStream();
                await blobClient.DownloadToAsync(memStream);
                memStream.Position = 0;
                var streamContent = new StreamContent(memStream);

                var multipartFormDataContent = new MultipartFormDataContent
                {
                    { new StringContent(YammerNetworkId), "network_id" },
                    { new StringContent(YammerGroupId), "group_id" },
                    { new StringContent("GROUP"), "target_type" },
                    { streamContent, "file"},
                    { new StringContent(fileWithoutPath), "filename" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, YammerFileUploadUrl);
                request.Content = multipartFormDataContent;
                var response = await HttpClient.SendAsync(request);

                var statusCode = response.StatusCode;
                var content = await response.Content.ReadAsStringAsync();

                if (statusCode == HttpStatusCode.Created)
                {
                    var json = JObject.Parse(content);

                    var id = json["id"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        return (true, id);
                    }
                }
                
                //if (statusCode == HttpStatusCode.TooManyRequests)
                //{
                // handle throttling if needed
                //}
                else if (statusCode == HttpStatusCode.BadRequest && content.Contains("FileSizeLargerThanAllowed"))
                {
                    Console.Write($" | File size is too large ({fileWithoutPath}) - manually uploaded to SharePoint");
                    return (false, null);
                }
                else if (statusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"\n\nEXPIRED access token for {rowKey}, exiting");

                    post.Status = "Error";
                    post.MigrationComment = $"Access token expired";
                    await TableClient.UpdateEntityAsync(post, ETag.All);

                    Console.ReadLine();
                    Console.ReadLine();
                    Environment.Exit(1);
                }

                response.EnsureSuccessStatusCode();

                Console.WriteLine($"\n\nERROR for {rowKey}: File id not found for {Path.GetFileName(filename)}");
                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\nERROR for {rowKey}: uploading small file {fileWithoutPath}: {ex.Message}");
                return (false, null);
            }
        }
    }
}

public class EngagePost : ITableEntity
{
    public EngagePost()
    {
    }

    public EngagePost(string userId, string userName, string postId, DateTime created, string? message, string groupName, string fileName)
    {
        PartitionKey = "Engage";
        RowKey = postId;

        Created = created;
        GroupName = groupName;
        FileName = fileName;
        Message = message;
        PostId = postId;
        Status = "To migrate";
        UserId = userId;
        UserName = userName;
    }

    public DateTime Created { get; set; }
    public string? FileName { get; set; }
    public string? GroupName { get; set; }
    public string? Message { get; set; }
    public string? MigrationComment { get; set; }
    public string? PhotoLocations { get; set; }
    public string? PostId { get; set; }
    public string? Status { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? VideoLocations { get; set; }

    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}