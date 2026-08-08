namespace LeakyBucketApp
{
    public sealed class PDFGenerator(int maxPerSecond) : IDisposable
    {
        private readonly Queue<DateTime> callTimestamps = new();
        private readonly SemaphoreSlim generateSemaphore = new SemaphoreSlim(1);

        public void Dispose() => generateSemaphore?.Dispose();

        public void GeneratePDF(CorrespondenceDocument _, CancellationToken cancellationToken = default)
        {
            generateSemaphore.Wait(cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-1);

                while (callTimestamps.Count > 0 && callTimestamps.Peek() < windowStart)
                {
                    callTimestamps.Dequeue();
                }

                if (callTimestamps.Count >= maxPerSecond)
                {
                    throw PDFGenerationThroughputExceededException.PerSecondExceeded(maxPerSecond);
                }

                callTimestamps.Enqueue(now);
            }
            finally
            {
                generateSemaphore.Release();
            }
        }
    }
}
