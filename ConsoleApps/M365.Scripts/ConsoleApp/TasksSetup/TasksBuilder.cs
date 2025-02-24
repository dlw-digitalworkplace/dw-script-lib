using ConsoleApp.TasksSetup;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Tasks
{
    public class TasksBuilder : IHostedService
    {
        private readonly ILogger<TasksBuilder> _logger;
        private readonly IServiceProvider _serviceProvider;
        public TasksBuilder(ILogger<TasksBuilder> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This methode is the start of the console app. It generates a menu of all script options and starts executing the selected script.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var option = EnumExtension.PrintMenu<ScriptTask>("Select a script to execute.");
            return Task.Run(async () =>
            {
                try
                {
                    await _serviceProvider.GetRequiredKeyedService<IScriptTask>(option).RunAsync();
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WebJob stopping at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
