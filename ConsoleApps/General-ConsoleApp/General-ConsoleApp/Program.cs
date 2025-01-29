using General_ConsoleApp.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;

            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                config.AddUserSecrets<Program>();
            }

            config.AddEnvironmentVariables();

        }).ConfigureServices((context, services) =>
        {
            DependencyInjection.InjectDependencies(services);

            services.AddSingleton(context.Configuration);
            services.AddLogging(configure => configure.AddConsole());
            services.AddMemoryCache();
        })
        .Build();

        await builder.RunAsync();
    }
}