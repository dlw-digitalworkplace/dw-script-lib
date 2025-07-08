using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using PnP.Framework;
using PnP.Framework.Entities;
using Services.Interfaces.SharePoint;

namespace Services.SharePoint
{
    public class SharePointService : ISharePointService
    {
        private readonly IConfiguration _configuration;
        private readonly ISharePointContextProvider _sharePointContextProvider;

        public SharePointService(ISharePointContextProvider sharePointContextProvider, IConfiguration configuration)
        {
            _configuration = configuration;
            _sharePointContextProvider = sharePointContextProvider;
        }

        // Check connect for client context creation with certificate
        public async Task<ClientContext> CreateClientContextAsync(string siteUrl)
        {
            var context = await _sharePointContextProvider.CreateAsync(siteUrl);
            return context;
        }

        public async Task<ClientContext> CreateClientContextAsync(string siteUrl, string appId, string appSecret)
        {
            var authManager = new AuthenticationManager();

            var context = authManager.GetACSAppOnlyContext(siteUrl, appId, appSecret);

            return context;
        }

        public async Task UpdateCustomScriptingAsync(string siteUrl, DenyAddAndCustomizePagesStatus denyAddAndCustomizePagesStatus)
        {
            using (var context = await CreateClientContextAsync($"https://{_configuration[Globals.Tenant.Name]}-admin.sharepoint.com/"))
            {
                var tenant = new Tenant(context);

                var siteProperties = tenant.GetSitePropertiesByUrl(siteUrl, false);
                context.Load(siteProperties);
                context.ExecuteQueryRetry();

                siteProperties.DenyAddAndCustomizePages = denyAddAndCustomizePagesStatus;
                siteProperties.Update();
                context.ExecuteQueryRetry();
            }
        }

        public async Task<SiteLockState> UpdateSiteLockStateAsync(ClientContext clientContext, string siteUrl, SiteLockState siteLockState)
        {
            var tenant = new Tenant(clientContext);

            var siteProperties = tenant.GetSitePropertiesByUrl(siteUrl, true);
            clientContext.Load(siteProperties);
            await clientContext.ExecuteQueryRetryAsync();

            var currentSiteLockState = (SiteLockState)Enum.Parse(typeof(SiteLockState), siteProperties.LockState);

            if (!siteProperties.LockState.Equals(siteLockState))
            {
                tenant.SetSiteLockState(siteUrl, siteLockState);
                await clientContext.ExecuteQueryRetryAsync();
            }

            return currentSiteLockState;
        }

        public async Task SetVersionHistoryLimitOnListAsync(ClientContext context, List list, VersionPolicyTrimMode versionPolicyTrimMode, int expireVersionsAfterDays = 30)
        {
            if (list.EnableVersioning && !list.Hidden && list.BaseType == BaseType.DocumentLibrary)
            {
                list.VersionPolicies.DefaultTrimMode = versionPolicyTrimMode;

                if (VersionPolicyTrimMode.ExpireAfter == versionPolicyTrimMode)
                {
                    list.VersionPolicies.DefaultExpireAfterDays = expireVersionsAfterDays;
                }

                list.Update();
                await context.ExecuteQueryRetryAsync();
            }
        }

        public async Task SetVersionHistoryLimitOnSiteAsync(string siteUrl, VersionPolicyTrimMode versionPolicyTrimMode, int expireVersionsAfterDays = 30)
        {
            try
            {
                using (var context = await CreateClientContextAsync($"https://{_configuration[Globals.Tenant.Name]}-admin.sharepoint.com/"))
                {
                    var tenant = new Tenant(context);

                    var siteProperties = tenant.GetSitePropertiesByUrl(siteUrl, false);
                    context.Load(siteProperties);
                    context.ExecuteQueryRetry();

                    switch (versionPolicyTrimMode)
                    {
                        case VersionPolicyTrimMode.NoExpiration:
                            siteProperties.EnableAutoExpirationVersionTrim = false;
                            break;
                        case VersionPolicyTrimMode.AutoExpiration:
                            siteProperties.EnableAutoExpirationVersionTrim = true;
                            break;
                        case VersionPolicyTrimMode.ExpireAfter:
                            siteProperties.EnableAutoExpirationVersionTrim = false;
                            siteProperties.ExpireVersionsAfterDays = expireVersionsAfterDays;
                            break;
                        case VersionPolicyTrimMode.Other:
                            break;
                    }

                    siteProperties.Update();
                    context.ExecuteQueryRetry();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public async Task<ListCollection> GetAllSiteLists(ClientContext context)
        {
            var lists = context.Web.Lists;

            context.Load(lists, l => l.Include(
            list => list.Title,
            list => list.Hidden,
            list => list.BaseType));
            context.Load(lists);
            await context.ExecuteQueryAsync();

            return lists;
        }

        public async Task<List<ListItem>> GetAllListItems(ClientContext context, List list, CamlQuery? camlQuery = null)
        {
            var allListItems = new List<ListItem>();
            ListItemCollectionPosition? position = null;

            if (null == camlQuery)
            {
                camlQuery = CamlQuery.CreateAllItemsQuery(5000);
            }

            do
            {
                camlQuery.ListItemCollectionPosition = position;
                var currentListItemCollection = list.GetItems(camlQuery);
                context.Load(currentListItemCollection);
                await context.ExecuteQueryRetryAsync();

                foreach (var listItem in currentListItemCollection)
                {
                    allListItems.Add(listItem);
                }

                context.Load(currentListItemCollection, currentListItems => currentListItems.ListItemCollectionPosition);
                await context.ExecuteQueryAsync();
                position = currentListItemCollection.ListItemCollectionPosition;
            }
            while (position != null);

            return allListItems;
        }

        public async Task<IList<SiteEntity>> GetAllSiteCollectionsAsync(ClientContext context)
        {
            Tenant tenant = new Tenant(context);
            var sites = tenant.GetSiteCollections();
            await context.ExecuteQueryRetryAsync();

            return sites;
        }

        public async Task<IEnumerable<string>> GetAllSiteCollectionURLsByPrefixAsync(ClientContext context)
        {
            var keywordQuery = new KeywordQuery(context)
            {
                QueryText = $"path:https://{_configuration[Globals.Tenant.Name]}.sharepoint.com/sites/{_configuration[Globals.SP.SitePrefix]}*",
                TrimDuplicates = false
            };

            var searchExecutor = new SearchExecutor(context);
            var results = searchExecutor.ExecuteQuery(keywordQuery);
            await context.ExecuteQueryRetryAsync();

            var filteredSites = results.Value[0].ResultRows.Select(r => r["Path"].ToString());
            return filteredSites;
        }

        public async Task<WebCollection> GetSubsitesAsync(ClientContext context)
        {
            var subSites = context.Web.Webs;

            context.Load(subSites, sites => sites.Include(
                site => site.Url,
                site => site.Title));
            await context.ExecuteQueryRetryAsync();

            return subSites;
        }
    }
}
