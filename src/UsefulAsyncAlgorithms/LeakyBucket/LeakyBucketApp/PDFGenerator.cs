namespace LeakyBucketApp
{
    public sealed class PDFGenerator(int maxPerSecond) : IDisposable
    {
        private readonly Queue<DateTime> callTimestamps = new();
        private readonly SemaphoreSlim generateSemaphore = new SemaphoreSlim(1);

        public void Dispose() => generateSemaphore?.Dispose();

        public async Task GeneratePDFAsync(CorrespondanceDocument _, CancellationToken cancellationToken = default)
        {
            try
            {
                await generateSemaphore.WaitAsync(cancellationToken);

                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-1);

                while (callTimestamps.Count > 0 && callTimestamps.Peek() < windowStart)
                {
                    callTimestamps.Dequeue();
                }

                if (callTimestamps.Count >= maxPerSecond)
                {
                    throw PDFGenerationThroughputExceededException.PerMinuteExceeded(maxPerSecond);
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