using Azure.Storage.Queues;

namespace Services.Interfaces
{
    public interface IQueueService
    {
        QueueClient GetQueue();

        Task SendQueueMessageAsync(object message);
    }
}
