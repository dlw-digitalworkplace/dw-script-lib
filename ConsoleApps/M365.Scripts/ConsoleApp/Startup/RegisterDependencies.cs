using Core.Enums;
using ConsoleApp.TasksSetup;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Interfaces.SharePoint;
using Services.Services;
using Services.Services.SharePoint;
using Services.SharePoint;
using ConsoleApp.Tasks;

namespace ConsoleApp.Startup
{
    public static class RegisterDependencies
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ISharePointService, SharePointService>();
            services.AddSingleton<ISharePointContextProvider, SharePointContextProvider>();
            services.AddSingleton<IAccessTokenProvider, CertificateStoreAccessTokenProvider>();
        }

        public static void RegisterKeyedServices(this IServiceCollection services)
        {
            services.AddHostedService<TasksBuilder>();
            services.AddKeyedSingleton<IScriptTask, QuickExecutionTask>(ScriptTask.QuickExecution);
        }
    }
}
