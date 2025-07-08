namespace Core.Extensions
{
    public static class ListExtension
    {
        public static async Task ExecuteInBatchAsync<T>(this List<T> list, int batchSize, Func<T, Task> itemAction)
        {
            if (batchSize <= 1)
            {
                foreach (var item in list)
                {
                    await itemAction(item);
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i += batchSize)
                {
                    var tasks = new List<Task>();
                    var currentBatchItems = list.GetRange(i, Math.Min(batchSize, list.Count - i));

                    foreach (var item in currentBatchItems)
                    {
                        tasks.Add(itemAction(item));
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }
    }
}
