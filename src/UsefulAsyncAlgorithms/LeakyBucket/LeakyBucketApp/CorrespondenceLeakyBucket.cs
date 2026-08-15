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
    public sealed class CorrespondenceLeakyBucket(
        TimeSpan leakInterval,
        int capacity) : IDisposable
    {
        private readonly ConcurrentQueue<CorrespondenceDocument> queue = new();
        private readonly SemaphoreSlim signal = new(0);
        private readonly Subject<CorrespondenceDocument> subject = new();
        private bool completed;

        public IObservable<CorrespondenceDocument> LeakyStream => subject;

        public void AddToBucket(CorrespondenceDocument document)
        {
            if (completed)
            {
                throw new InvalidOperationException("Cannot add to a completed leaky bucket.");
            }

            var newCount = queue.Count + 1;
            if (newCount > capacity)
            {
                throw BucketOverflowException.CapacityExceeded(newCount, capacity);
            }

            queue.Enqueue(document);
            signal.Release();
        }

        public void Complete()
        {
            completed = true;
            signal.Release();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            subject.Dispose();
            signal.Dispose();
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
                else if (completed)
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
    }
}