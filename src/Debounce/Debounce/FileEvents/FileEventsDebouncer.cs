using System.Reactive.Subjects;

namespace Debounce.FileEvents
{
    public class FileEventsDebouncer(
        Publisher<FileEvent> publisher,
        TimeSpan window) : IDisposable
    {
        private readonly Publisher<FileEvent> publisher = publisher;
        private readonly Subject<FileEvent> subject = new();
        private readonly TimeSpan window = window;
        public IObservable<FileEvent> FileEventStream => subject;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            subject.Dispose();
            publisher.Dispose();
        }
    }
}