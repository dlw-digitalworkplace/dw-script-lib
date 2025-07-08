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
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var option = EnumExtension.PrintMenu<ScriptTask>("Select a script to execute.");

                try
                {
                    var scriptTask = _serviceProvider.GetRequiredKeyedService<IScriptTask>(option);
                    await scriptTask.RunAsync();

                    Console.WriteLine("Task finished. Press any key to go back to the menu.");
                    Console.ReadKey(true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("Cancellation requested. Exiting menu.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WebJob stopping at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
