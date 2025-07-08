using ConsoleApp.TasksSetup;
using Core;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Services.Interfaces;
using Services.Interfaces.SharePoint;

namespace ConsoleApp.Tasks
{
    public class VersionCleanUpTask : IScriptTask
    {
        private readonly ISharePointService _sharePointService;
        private readonly IConfigurationService _configurationService;
        private readonly IConfiguration _configuration;

        public VersionCleanUpTask(ISharePointService sharePointService, IConfigurationService configurationService, IConfiguration configuration)
        {
            _sharePointService = sharePointService;
            _configurationService = configurationService;
            _configuration = configuration;
        }

        public async Task RunAsync()
        {
            var dataSource = _configuration[Globals.General.DataSource.Option].TryParseToEnum<DataSource>();
            var batchSize = _configuration[Globals.General.BatchSize].TryParseToInt();

            var sharepointSiteURLs = await _configurationService.GetSharePointSiteURLs(dataSource);

            foreach(var siteUrl in sharepointSiteURLs)
            {
                if (string.IsNullOrWhiteSpace(siteUrl)) { continue; }

                var context = await _sharePointService.CreateClientContextAsync(siteUrl);
                await CleanUpVersions(context, batchSize);
            }
        }

        private async Task CleanUpVersions(ClientContext context, int batchSize)
        {
            var keepAmount = _configuration[Globals.Task.IntelligentVersioning.KeepAmount].TryParseToIntOrDefault();
            var expireVersionsAfterDays = _configuration[Globals.Task.IntelligentVersioning.ExpireVersionsAfterDays].TryParseToIntOrDefault();
            var lists = (await _sharePointService.GetAllSiteLists(context)).Where(list => !list.Hidden).ToList();

            foreach (var list in lists)
            {
                var listItems = await _sharePointService.GetAllListItems(context, list);

                await listItems.ExecuteInBatchAsync(batchSize, async listItem =>
                {
                    await RemoveFileVersions(context, listItem, keepAmount, expireVersionsAfterDays);
                });
            }

            var subsites = await _sharePointService.GetSubsitesAsync(context);
            foreach (var subsite in subsites)
            {
                var subSiteContext = await _sharePointService.CreateClientContextAsync(subsite.Url);
                await CleanUpVersions(subSiteContext, batchSize);
            }
        }

        private async Task RemoveFileVersions(ClientContext context, ListItem listItem, int keepAmount, int expireVersionsAfterDays)
        {
            if (0 == keepAmount && 0 == expireVersionsAfterDays)
            {
                listItem.File.Versions.DeleteAll();
                await context.ExecuteQueryRetryAsync();
            }
            else
            {
                try
                {
                    // Remove file versions based on config
                    context.Load(listItem, listItem => listItem.File.Name);
                    context.Load(listItem.File.Versions);
                    await context.ExecuteQueryAsync();
                    var allVersions = listItem.File.Versions.OrderByDescending(version => version.Created).ToList();
                    if (0 != keepAmount)
                    {
                        allVersions = allVersions.Skip(keepAmount).ToList();
                    }
                    if (0 != expireVersionsAfterDays)
                    {
                        // Change to months
                        var newerThenDate = DateTime.UtcNow.AddDays(-expireVersionsAfterDays);
                        allVersions = allVersions.Where(version => 1 > DateTime.Compare(version.Created, newerThenDate)).ToList();
                    }

                    foreach (var version in allVersions)
                    {
                        try
                        {
                            version.DeleteObject();
                        }
                        catch (Exception ex)
                        {
                            var x = ex;
                        }
                    }
                    await context.ExecuteQueryRetryAsync();
                }
                catch (Exception ex)
                {
                    var e = ex;
                }
            }
        }
    }
}
