using Core.Enums;
using General_ConsoleApp.TasksSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Scripts.Tasks
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Select a script to run.");
                ScriptTaskMenuOptionsHelper.CreateMenu();
                var input = Console.ReadLine();
                if (null == input)
                {
                    throw new Exception();
                }
                var option = (ScriptTask)int.Parse(input);
                return Task.Run(async () =>
                {
                    await _serviceProvider.GetRequiredKeyedService<IScriptTask>(option).RunAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while running the script");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WebJob stopping at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
