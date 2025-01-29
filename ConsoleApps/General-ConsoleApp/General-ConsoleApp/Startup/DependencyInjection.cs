using Core.Enums;
using General_ConsoleApp.TasksSetup;
using Microsoft.Extensions.DependencyInjection;
using Scripts.Tasks;
using Services.Interfaces;
using Services.Interfaces.SharePoint;
using Services.Services;
using Services.Services.SharePoint;
using Services.SharePoint;

namespace General_ConsoleApp.Startup
{
    public static class DependencyInjection
    {
        public static void InjectDependencies(IServiceCollection services)
        {
            // Services
            services.AddScoped<ISharePointService, SharePointService>();
            services.AddSingleton<ISharePointContextProvider, SharePointContextProvider>();
            services.AddSingleton<IAccessTokenProvider, CertificateStoreAccessTokenProvider>();

            // Keyed services used for scripts
            services.AddHostedService<TasksBuilder>();
            services.AddKeyedSingleton<IScriptTask, QuickExecutionTask>(ScriptTask.QuickExecution);
        }
    }
}
