using System.Reactive.Subjects;

namespace Common
{
    public class Publisher<TPublishable>(
        IEnumerable<TPublishable> publishables,
        TimeSpan tickDelay,
        int batchSize = 1) : IDisposable where TPublishable : IPublishable<TPublishable>
    {
        private readonly Queue<TPublishable> queue = new Queue<TPublishable>(publishables);
        private readonly Subject<TPublishable> subject = new();
        public IObservable<TPublishable> MessageStream => subject;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public async Task StartPublishingAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < batchSize && queue.Count > 0; i++)
                {
                    var item = queue.Dequeue();

                    var next = item.Next();
                    subject.OnNext(next);

                    queue.Enqueue(item);
                }

                try
                {
                    await Task.Delay(tickDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        protected virtual void Dispose(bool disposing) => subject.Dispose();
    }
}