using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace UsefulAsyncAlgorithms.Debounce
{
    /// <summary>
    /// Simple window-based debouncer for file events.
    /// Emits events only once per file path within a specified time window.
    /// Useful for filtering out rapid duplicate events for the same file.
    /// </summary>
    public class FileEventsDebouncer : IDisposable
    {
        private readonly IDisposable cleanUpSub;

        /// <summary>
        /// Events with the same path within this time span are ignored.
        /// </summary>,
        private readonly TimeSpan debounceWindow;

        // Stores the last event time for each file path to support debouncing.
        private readonly ConcurrentDictionary<string, DateTime> lastEventTimes = new();

        private readonly Publisher<FileEvent> publisher;
        private readonly Subject<FileEvent> subject = new();
        private readonly IDisposable subscription;
        public IObservable<FileEvent> FileEventStream => subject;

        public FileEventsDebouncer(Publisher<FileEvent> publisher, TimeSpan debounceWindow, TimeSpan cleanUpInterval)
        {
            this.publisher = publisher;
            this.debounceWindow = debounceWindow;
            subscription = publisher.PublishableStream.Subscribe(OnNext);
            cleanUpSub = Observable.Interval(cleanUpInterval).Subscribe(CleanUp);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            subscription.Dispose();
            subject.Dispose();
            publisher.Dispose();
            cleanUpSub.Dispose();
        }

        /// <summary>
        /// lastEventTimes dictionary should be periodically cleaned up
        /// to avoid memory leaks from paths that are no longer active.
        /// this is example clean up
        /// </summary>
        private void CleanUp(long obj)
        {
            var threshold = DateTime.UtcNow - debounceWindow;
            foreach (var kvp in lastEventTimes)
            {
                // Remove entries older than the threshold
                if (kvp.Value < threshold)
                {
                    lastEventTimes.TryRemove(kvp.Key, out _);
                }
            }
        }

        /// <summary>
        /// Handles incoming events and applies debouncing based on the window and path.
        /// </summary>
        private void OnNext(FileEvent fileEvent)
        {
            if (lastEventTimes.TryGetValue(fileEvent.Path, out var lastTime))
            {
                // Check if the event is within the debounce window
                if (fileEvent.PublishTime - lastTime < debounceWindow)
                {
                    // Ignore event within deboucing window
                    return;
                }
            }

            // Publish event and update last event time for path
            lastEventTimes[fileEvent.Path] = fileEvent.PublishTime;
            subject.OnNext(fileEvent);
        }
    }
}