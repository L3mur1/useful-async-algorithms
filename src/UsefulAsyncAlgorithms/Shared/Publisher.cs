using System.Reactive.Subjects;

namespace Common
{
    public class Publisher<TMessage>(IEnumerable<TMessage> publishables) : IDisposable
        where TMessage : IPublishable<TMessage>
    {
        private readonly Queue<TMessage> queue = new Queue<TMessage>(publishables);
        private readonly Subject<TMessage> subject = new();
        public IObservable<TMessage> MessageStream => subject;

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