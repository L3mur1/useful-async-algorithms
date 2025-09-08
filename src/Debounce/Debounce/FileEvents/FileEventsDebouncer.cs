using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Debounce.FileEvents
{
    /// <summary>
    /// Simple window-based debouncer for file events.
    /// Emits events only once per file path within a specified time window.
    /// Useful for filtering out rapid duplicate events for the same file.
    /// </summary>
    public class FileEventsDebouncer : IDisposable
    {
        /// <summary>
        /// Events with the same path within this time span are ignored.
        /// </summary>,
        private readonly TimeSpan debounceWindow;

        // Stores the last event time for each file path to support debouncing.
        // NOTE: In a real application, this dictionary should be periodically cleaned up
        // to avoid memory leaks from paths that are no longer active.
        private readonly ConcurrentDictionary<string, DateTime> lastEventTimes = new();

        private readonly Publisher<FileEvent> publisher;
        private readonly Subject<FileEvent> subject = new();
        private readonly IDisposable subscription;
        public IObservable<FileEvent> FileEventStream => subject;

        public FileEventsDebouncer(Publisher<FileEvent> publisher, TimeSpan debounceWindow)
        {
            this.publisher = publisher;
            this.debounceWindow = debounceWindow;
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

        /// <summary>
        /// Handles incoming events and applies debouncing based on the window and path.
        /// </summary>
        private void OnNext(FileEvent fileEvent)
        {
            var now = fileEvent.PublishTime;
            if (lastEventTimes.TryGetValue(fileEvent.Path, out var lastTime))
            {
                if (now - lastTime < debounceWindow)
                {
                    // Ignore event within deboucing window
                    return;
                }
            }

            lastEventTimes[fileEvent.Path] = now;
            subject.OnNext(fileEvent);
        }
    }
}