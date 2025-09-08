using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Debounce.FileEvents
{
    public class FileEventsDebouncer : IDisposable
    {
        private readonly ConcurrentDictionary<string, DateTime> lastEventTimes = new();
        private readonly Publisher<FileEvent> publisher;
        private readonly Subject<FileEvent> subject = new();
        private readonly IDisposable subscription;
        private readonly TimeSpan window;
        public IObservable<FileEvent> FileEventStream => subject;

        public FileEventsDebouncer(Publisher<FileEvent> publisher, TimeSpan window)
        {
            this.publisher = publisher;
            this.window = window;
            subscription = publisher.PublishableStream.Subscribe(OnNext);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                subscription.Dispose();
                subject.Dispose();
                publisher.Dispose();
            }
        }

        private void OnNext(FileEvent fileEvent)
        {
            var now = fileEvent.PublishTime;
            if (lastEventTimes.TryGetValue(fileEvent.Path, out var lastTime))
            {
                if (now - lastTime < window)
                {
                    // Ignore event within window
                    return;
                }
            }

            lastEventTimes[fileEvent.Path] = now;
            subject.OnNext(fileEvent);
        }
    }
}