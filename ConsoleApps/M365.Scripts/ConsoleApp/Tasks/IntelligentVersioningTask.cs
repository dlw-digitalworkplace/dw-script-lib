using ConsoleApp.TasksSetup;
using Core;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using Services.Interfaces;
using Services.Interfaces.SharePoint;

namespace ConsoleApp.Tasks
{
    /// <summary>
    /// This task will apply version history limit to all defined sites and document libraries
    /// Microsoft docs: https://learn.microsoft.com/en-us/sharepoint/set-default-org-version-limits
    /// </summary>
    public class IntelligentVersioningTask : IScriptTask
    {
        private readonly ISharePointService _sharePointService;
        private readonly IConfigurationService _configurationService;
        private readonly IConfiguration _configuration;

        public IntelligentVersioningTask(ISharePointService sharePointService, IConfigurationService configurationService, IConfiguration configuration)
        {
            _sharePointService = sharePointService;
            _configurationService = configurationService;
            _configuration = configuration;
        }

        public async Task RunAsync()
        {
            var dataSource = _configuration[Globals.General.DataSource.Option].TryParseToEnum<DataSource>();
            var batchSize = _configuration[Globals.General.BatchSize].TryParseToInt();
            var versionPolicyTrimMode = _configuration[Globals.SP.VersionPolicyTrimMode].TryParseToEnum<VersionPolicyTrimMode>();
            var expireVersionsAfterDays = _configuration[Globals.Task.IntelligentVersioning.ExpireVersionsAfterDays].TryParseToIntOrDefault();

            var sharepointSiteURLs = await _configurationService.GetSharePointSiteURLs(dataSource);

            foreach (var siteUrl in sharepointSiteURLs)
            {
                if (string.IsNullOrWhiteSpace(siteUrl)) { continue; }

                try
                {
                    await _sharePointService.UpdateCustomScriptingAsync(siteUrl, DenyAddAndCustomizePagesStatus.Disabled);

                    var context = await _sharePointService.CreateClientContextAsync(siteUrl);
                    //var siteLock = await _sharePointService.UpdateSiteLockStateAsync(context, siteUrl, SiteLockState.Unlock);

                    await _sharePointService.SetVersionHistoryLimitOnSiteAsync(siteUrl, versionPolicyTrimMode, expireVersionsAfterDays);

                    await SetVersionHistoryLimitOnListsAndSubSites(context, batchSize, versionPolicyTrimMode, expireVersionsAfterDays);

                    //await _sharePointService.UpdateSiteLockStateAsync(context, siteUrl, siteLock);
                }
                catch (Exception)
                {

                }
            }
        }

        private async Task SetVersionHistoryLimitOnListsAndSubSites(ClientContext context, int batchSize, VersionPolicyTrimMode versionPolicyTrimMode, int expireVersionsAfterDays)
        {
            var siteListCollection = await _sharePointService.GetAllSiteLists(context);
            var allSiteLists = siteListCollection.Where(l => !l.Hidden).ToList();

            await allSiteLists.ExecuteInBatchAsync(batchSize, async list =>
            {
                await _sharePointService.SetVersionHistoryLimitOnListAsync(context, list, versionPolicyTrimMode, expireVersionsAfterDays);
            });

            var subsites = await _sharePointService.GetSubsitesAsync(context);
            foreach (var subsite in subsites)
            {
                var subSiteContext = await _sharePointService.CreateClientContextAsync(subsite.Url);
                await SetVersionHistoryLimitOnListsAndSubSites(subSiteContext, batchSize, versionPolicyTrimMode, expireVersionsAfterDays);
            }
        }
    }
}
