using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using PnP.Framework.Entities;

namespace Services.Interfaces.SharePoint
{
    public interface ISharePointService
    {
        Task<ClientContext> CreateClientContextAsync(string site);
        Task<ClientContext> CreateClientContextAsync(string siteUrl, string appId, string appSecret);
        Task<SiteLockState> UpdateSiteLockStateAsync(ClientContext clientContext, string siteUrl, SiteLockState siteLockState);
        Task UpdateCustomScriptingAsync(string siteUrl, DenyAddAndCustomizePagesStatus denyAddAndCustomizePagesStatus);
        Task SetVersionHistoryLimitOnSiteAsync(string siteUrl, VersionPolicyTrimMode versionPolicyTrimMode, int expireVersionsAfterDays = 30);
        Task SetVersionHistoryLimitOnListAsync(ClientContext context, List list, VersionPolicyTrimMode versionPolicyTrimMode, int expireVersionsAfterDays = 30);
        Task<ListCollection> GetAllSiteLists(ClientContext context);
        Task<List<ListItem>> GetAllListItems(ClientContext context, List list, CamlQuery? camlQuery = null);
        Task<IList<SiteEntity>> GetAllSiteCollectionsAsync(ClientContext context);
        Task<IEnumerable<string>> GetAllSiteCollectionURLsByPrefixAsync(ClientContext context);
        Task<WebCollection> GetSubsitesAsync(ClientContext context);
    }
}
