using ConsoleApp.TasksSetup;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Services.Interfaces.SharePoint;

namespace ConsoleApp.Tasks
{
    /// <summary>
    /// This script can be used to validate some small actions, test some things,...
    /// </summary>
    public class QuickExecutionTask : IScriptTask
    {
        private readonly IConfiguration _configuration;
        private readonly ISharePointService _sharePointService;
        public QuickExecutionTask(ISharePointService sharePointService, IConfiguration configuration)
        {
            _sharePointService = sharePointService;
            _configuration = configuration;
        }

        public async Task RunAsync()
        {
            using(var context = await _sharePointService.CreateClientContextAsync("https://imecinternational.sharepoint.com/sites/imec-psi", "37167755-54a6-430a-911c-4b691584834c", "69:5/wrXT[KwqC[kaYp8M89t6@FbTh9n"))
            {
                var roleAssignments = context.Web.RoleAssignments;
                context.Load(roleAssignments, roles => roles.Include(ra => ra.Member, ra => ra.PrincipalId));
                context.ExecuteQueryRetry();

                var x = roleAssignments[10].Member.Title;
            }
        }
    }
}
