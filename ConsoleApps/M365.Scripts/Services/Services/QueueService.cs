using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class QueueService : IQueueService
    {
        private readonly QueueClient queue;
        public QueueService(IConfiguration configuration)
        {
            queue = new QueueClient(configuration.GetConnectionString("AzureWebJobsStorage"), configuration.GetValue<string>("QueueName"));
        }
        public QueueClient GetQueue()
        {
            return queue;
        }

        public async Task SendQueueMessageAsync(object message)
        {
            await queue.SendMessageAsync(JsonConvert.SerializeObject(message));
        }   
    }
}
