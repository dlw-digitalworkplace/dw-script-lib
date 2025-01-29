using General_ConsoleApp.TasksSetup;
using Microsoft.Extensions.Configuration;
using Services.Interfaces.SharePoint;

namespace Scripts.Tasks
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
            // Implement your code here
        }
    }
}
