using System.Reactive.Subjects;

namespace Debounce
{
    public class Publisher<TPublishable>(IEnumerable<TPublishable> publishables) : IDisposable
        where TPublishable : IPublishable<TPublishable>
    {
        private readonly Queue<TPublishable> queue = new Queue<TPublishable>(publishables);
        private readonly Subject<TPublishable> subject = new();
        public IObservable<TPublishable> PublishableStream => subject;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public async Task StartPublishingAsync(TimeSpan eventsDelay, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = queue.Dequeue();

                var next = item.CreateNext();
                subject.OnNext(next);

                queue.Enqueue(item);

                try
                {
                    await Task.Delay(eventsDelay, cancellationToken);
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