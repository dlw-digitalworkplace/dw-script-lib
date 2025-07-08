using Core.Enums;
using ConsoleApp.TasksSetup;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Interfaces.SharePoint;
using Services.Services;
using Services.Services.SharePoint;
using Services.SharePoint;
using ConsoleApp.Tasks;
using Microsoft.Extensions.Configuration;
using Core;
using Azure.Storage.Blobs;

namespace ConsoleApp.Startup
{
    public static class RegisterDependencies
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISharePointService, SharePointService>();
            services.AddScoped<ILocalFileService, LocalFileService>();
            services.AddSingleton<ISharePointContextProvider, SharePointContextProvider>();
            services.AddSingleton<IAccessTokenProvider, CertificateStoreAccessTokenProvider>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IBlobService, BlobService>();
            services.AddSingleton(new BlobServiceClient(GetConnectionString(configuration[Globals.AzureBlob.ConnectionString])));
        }

        public static void RegisterKeyedServices(this IServiceCollection services)
        {
            services.AddHostedService<TasksBuilder>();
            services.AddKeyedSingleton<IScriptTask, QuickExecutionTask>(ScriptTask.QuickExecution);
            services.AddKeyedSingleton<IScriptTask, IntelligentVersioningTask>(ScriptTask.IntelligentVersioning);
            services.AddKeyedSingleton<IScriptTask, VersionCleanUpTask>(ScriptTask.VersionCleanUp);
        }

        public static string GetConnectionString(string? connectionString)
        {
            return !string.IsNullOrWhiteSpace(connectionString)
                ? connectionString
                : "UseDevelopmentStorage=true";
        }
    }
}
