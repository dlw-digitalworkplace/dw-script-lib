using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.GetAllSites;

class GraphHelper
{
    // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
    // https://learn.microsoft.com/en-us/graph/tutorials/dotnet-app-only?tabs=aad&tutorial-step=3

    // The client credentials flow requires that you request the
    // /.default scope, and pre-configure your permissions on the
    // app registration in Azure. An administrator must grant consent
    // to those permissions beforehand.
    private string[] _appOnlyScope { get; set; } = new[] { "https://graph.microsoft.com/.default" };
    private ClientSecretCredential _clientSecretCredential { get; set; }
    private GraphServiceClient _graphServiceClient { get; set; }
    private Settings? _settings { get; set; }

    public GraphHelper(Settings settings)
    {
        _settings = settings;
        _clientSecretCredential = new ClientSecretCredential(_settings?.Graph?.TenantId, _settings?.Graph?.ClientId, _settings?.Graph?.ClientSecret);
        _graphServiceClient = new GraphServiceClient(_clientSecretCredential, _appOnlyScope);
    }

    #region Sample methods

    public async Task SampleMethods()
    {
        var accessToken = await GetAppOnlyTokenAsync();
        Console.WriteLine($"Access token: {accessToken}");

        var sites = await GetSites();
        Console.WriteLine($"Found {sites.Count} sites");

        var siteId = string.Empty;

        foreach (var s in sites)
        {
            Console.WriteLine($"Site name: {s.Name}");

            if (s.Name!.Equals(_settings?.Item?.SiteName))
            {
                siteId = s.Id;
                break;
            }
        }

        var site = await GetSiteById(siteId);
        var lists = await GetLists(site!);
        var docLib = lists?.FirstOrDefault(l => l.DisplayName == _settings?.Item?.DocLibName);
        var driveItems = await GetDriveItems(siteId!, _settings?.Item?.DocLibName!); // Note: try to avoid fetching the driveItem from the list/listitem... Use driveItem all the way
    }

    public async Task<string> GetAppOnlyTokenAsync()
    {
        var context = new TokenRequestContext(_appOnlyScope);
        var response = await _clientSecretCredential.GetTokenAsync(context);
        return response.Token;
    }

    public async Task<List<DriveItem>> GetDriveItems(string siteId, string docLibName)
    {
        var licr = await _graphServiceClient.Sites[siteId].Drives.GetAsync();

        var drives = licr?.Value;
        var docLibrary = drives?.FirstOrDefault(l => l.Name == docLibName);
        var driveId = docLibrary?.Id;

        var rootDriveItem = await _graphServiceClient.Drives[driveId!].Root.GetAsync();
        var rootDriveId = rootDriveItem?.Id!;

        var itemsResponse = await _graphServiceClient.Drives[driveId!].Items[rootDriveId].Children.GetAsync();
        var driveItems = itemsResponse?.Value; // todo: iterate over ALL drive items (paging) - see GetSites() for example

        return driveItems!;
    }

    public async Task<List<List>?> GetLists(Site site)
    {
        var lists = await _graphServiceClient.Sites[site.Id].Lists.GetAsync(); // todo: iterate over all lists
        return lists?.Value;
    }

    public async Task<Site?> GetSiteById(string? siteId)
    {
        if (string.IsNullOrWhiteSpace(siteId))
        {
            return null;
        }

        var site = await _graphServiceClient.Sites[siteId].GetAsync();
        return site;
    }

    public async Task<List<Site>> GetSites()
    {
        var sitesResponse = await _graphServiceClient.Sites.GetAllSites.GetAsGetAllSitesGetResponseAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = ["id", "name", "webUrl", "siteCollection"];
            requestConfig.QueryParameters.Top = 100;
            requestConfig.QueryParameters.Orderby = ["name"];
            requestConfig.QueryParameters.Count = true; // Include count in the response
            //requestConfig.QueryParameters.Filter = $"siteCollection/hostname eq 'mytenant.sharepoint.com'";
            //config.QueryParameters.Search = null;
            //config.QueryParameters.Expand = null;
        });

        //var count = sitesResponse?.OdataCount;

        var sites = new List<Site>(); // paging explained: https://learn.microsoft.com/en-us/graph/sdks/paging?tabs=csharp

        var count = 0;
        var pauseAfter = 10;

        if (sitesResponse == null)
        {
            return sites;
        }

        var useIteratorPaging = true;

        if (useIteratorPaging)
        {
            // iterator paging:
            var pageIterator = PageIterator<Site, GetAllSitesGetResponse>.CreatePageIterator(
                _graphServiceClient,
                sitesResponse,
                // Callback executed for each item in the collection
                (site) =>
                {
                    sites.Add(site);
                    count++;
                    return count < pauseAfter;
                },
                // Used to configure subsequent page requests, any headers from the original request need to be readded here
                (reqInfo) =>
                {
                    // Re-add the header to subsequent requests
                    //req.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                    return reqInfo;
                }
            );

            while (pageIterator.State != PagingState.Complete)
            {
                Console.WriteLine($"Paused after {pauseAfter}, list count: {sites.Count}");
                count = 0; // Reset count
                await pageIterator.ResumeAsync();
            }
        }
        else
        {
            var nextLink = sitesResponse!.OdataNextLink;

            // manual paging:
            while (nextLink is not null)
            {
                nextLink = sitesResponse!.OdataNextLink;
                sites.AddRange(sitesResponse!.Value ?? []);

                if (!string.IsNullOrEmpty(sitesResponse.OdataNextLink))
                {
                    sitesResponse = await _graphServiceClient.Sites.GetAllSites
                        .WithUrl(sitesResponse.OdataNextLink)
                        .GetAsGetAllSitesGetResponseAsync();
                }
            }
        }

        return sites;
    }

    #endregion

    public async Task<Dictionary<DriveItem, List<Permission>>> GetPermissions(string siteId, string docLibName, string driveItemId)
    {
        var licr = await _graphServiceClient.Sites[siteId].Drives.GetAsync();

        var drives = licr?.Value;
        var docLibrary = drives?.FirstOrDefault(l => l.Name == docLibName);
        var driveId = docLibrary?.Id;

        var rootDriveItem = await _graphServiceClient.Drives[driveId!].Root.GetAsync();
        var rootDriveId = rootDriveItem?.Id!;

        var itemsResponse = await _graphServiceClient.Drives[driveId!].Items[rootDriveId].Children.GetAsync();
        var driveItems = itemsResponse?.Value;

        var dict = new Dictionary<DriveItem, List<Permission>>();

        foreach (var driveItem in driveItems!)
        {
            if (driveItem.Id != driveItemId)
            {
                continue;
            }

            var permissions = await _graphServiceClient.Drives[driveId!].Items[driveItem.Id].Permissions.GetAsync();
            // Contains 'Link' if it is a sharing link. Null for regular permissions granted
            // 'Link' property contains: scope (user, organization), type (view, edit), webUrl, preventsDownload, expirationDateTime, hasPassword
            // If 'Link' is not null, ExpirationDateTime might not be null, hasPassword is a boolean

            // Share id (should contain owner but does not...)
            //foreach (var p in permissions!.Value!)
            //{
            //    if (!p.Roles!.Any(r => r == "read"))
            //    {
            //        continue;
            //    }
            //    var sharedDriveItem = await _graphServiceClient.Shares[p.ShareId].GetAsync(); // does not contain user...
            //}

            foreach (var p in permissions!.Value!)
            {
                if (p.Link is not null) // Permission is a sharing link
                {
                    if (dict.ContainsKey(driveItem))
                    {
                        dict[driveItem].Add(p);
                    }
                    else
                    {
                        dict.Add(driveItem, [p]);
                    }

                    var role = p.Roles?.FirstOrDefault();
                    var username = p.GrantedToIdentitiesV2?.FirstOrDefault()?.User?.DisplayName;
                    var expDate = p.ExpirationDateTime;
                    var expDateString = expDate is null ? "" : "| Expires: " + expDate?.ToString("yyyy-MM-dd");

                    Console.WriteLine($"Item {driveItem.Name} has sharing link - user: {username} | role: {role} | type: {p.Link.Type} | scope: {p.Link.Scope} | prevents download: {p.Link.PreventsDownload} | has password: {p.HasPassword} {expDateString}");
                }
            }
        }

        return dict;
    }

}