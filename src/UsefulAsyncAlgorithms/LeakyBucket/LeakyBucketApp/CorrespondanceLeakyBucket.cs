using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace LeakyBucketApp
{
    /// <summary>
    /// Queues incoming correspondence and emits it at a constant leak rate,
    /// smoothing bursts so downstream PDF generation stays within throughput limits.
    /// Call <see cref="Complete"/> when no more items will be added; leaking then
    /// finishes after the queue has drained.
    /// </summary>
    public sealed class CorrespondanceLeakyBucket(TimeSpan leakInterval) : IDisposable
    {
        private readonly ConcurrentQueue<CorrespondanceDocument> queue = new();
        private readonly Subject<CorrespondanceDocument> subject = new();
        private readonly SemaphoreSlim signal = new(0);
        private int completed;

        public IObservable<CorrespondanceDocument> LeakyStream => subject;

        public void AddToBucket(CorrespondanceDocument document)
        {
            if (Volatile.Read(ref completed) == 1)
            {
                throw new InvalidOperationException("Cannot add to a completed leaky bucket.");
            }

            queue.Enqueue(document);
            signal.Release();
        }

        public void Complete()
        {
            if (Interlocked.Exchange(ref completed, 1) == 0)
            {
                signal.Release();
            }
        }

        public async Task StartLeakingAsync()
        {
            while (true)
            {
                if (queue.TryDequeue(out var document))
                {
                    subject.OnNext(document);
                    await Task.Delay(leakInterval);
                }
                else if (Volatile.Read(ref completed) == 1)
                {
                    subject.OnCompleted();
                    return;
                }
                else
                {
                    await signal.WaitAsync();
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                subject.Dispose();
                signal.Dispose();
            }
        }
    }
}