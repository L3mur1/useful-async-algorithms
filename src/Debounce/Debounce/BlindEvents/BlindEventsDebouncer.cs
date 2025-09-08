using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Debounce.BlindEvents
{
    public class BlindEventsDebouncer : IDisposable
    {
        private readonly Subject<BlindEvent> subject = new();
        private readonly IDisposable subscription;
        public IObservable<BlindEvent> EventStream => subject;

        public BlindEventsDebouncer(Publisher<BlindEvent> publisher)
        {
            subscription = publisher.PublishableStream.Subscribe(OnNext);
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