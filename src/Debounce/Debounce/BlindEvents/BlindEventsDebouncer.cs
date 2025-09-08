using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Debounce.BlindEvents
{
    public class BlindEventsDebouncer : IDisposable
    {
        private readonly int maxHeight;
        private readonly int minHeight;
        private readonly TimeSpan moveTimeout;
        private readonly Subject<BlindEvent> subject = new();
        private readonly IDisposable subscription;
        public IObservable<BlindEvent> EventStream => subject;

        public BlindEventsDebouncer(Publisher<BlindEvent> publisher, int minHeight, int maxHeight, TimeSpan moveTimeout)
        {
            subscription = publisher.PublishableStream.Subscribe(OnNext);
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.moveTimeout = moveTimeout;
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
        }

        private void OnNext(BlindEvent blindEvent)
        {
            throw new NotImplementedException();
        }
    }
}