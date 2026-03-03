using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ConsoleApp.TasksSetup;
using Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Services.Interfaces;
using Services.Interfaces.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Tasks
{
    public class SendQueueMessageTask : IScriptTask
    {
        private readonly IQueueService _queueClient;
        private readonly IConfiguration _configuration;
        public SendQueueMessageTask(IQueueService queueClient,IConfiguration configuration) {
            _queueClient = queueClient;
            _configuration = configuration;
        }
        public async Task RunAsync()
        {
            Console.WriteLine("What do you want to do : \nmessage(Sending queu messages)\nmail(sending mails)");
            var firstinput = Console.ReadLine();
            if (firstinput == "message")
            {
                var queue = GetQueue();

                var message = _configuration["QueueMessage"];
                Console.WriteLine($"Selected message : {message}");
                var input = Console.ReadLine();
                while (input != null)
                {

                    var queueMessage = JsonConvert.DeserializeObject(message);
                    Console.WriteLine($"Send {queueMessage}to queue!");

                    await SendMessageAsync(queueMessage);
                    input = Console.ReadLine();
                }
            }
        }

        private QueueClient GetQueue()
        {
            var queue = new QueueClient("UseDevelopmentStorage=true", "workspaces-afteraction-queue", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            Console.WriteLine($"Selected Queue : {queue.AccountName} | {queue.Name}");

            return queue;
        }
        private async Task SendMessageAsync(object message)
        {
            await _queueClient.SendQueueMessageAsync(JsonConvert.SerializeObject(message).ToString());
        }
    }
}
