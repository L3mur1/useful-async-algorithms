using System.
    Collections.Concurrent;
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
        private readonly SemaphoreSlim signal = new(0);
        private readonly Subject<CorrespondanceDocument> subject = new();
        private bool completed;

        public IObservable<CorrespondanceDocument> LeakyStream => subject;

        public void AddToBucket(CorrespondanceDocument document)
        {
            if (completed)
            {
                throw new InvalidOperationException("Cannot add to a completed leaky bucket.");
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