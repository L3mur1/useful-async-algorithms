namespace LeakyBucketApp
{
    public sealed class PDFGenerator(int maxPerMinute) : IDisposable
    {
        private readonly Queue<DateTime> callTimestamps = new();
        private readonly SemaphoreSlim generateSemaphore = new SemaphoreSlim(1);

        public void Dispose() => generateSemaphore?.Dispose();

        public async Task GeneratePDFAsync(CorrespondanceDocument _)
        {
            try
            {
                await generateSemaphore.WaitAsync();

                var now = DateTime.UtcNow;
                var windowStart = now.AddMinutes(-1);

                while (callTimestamps.Count > 0 && callTimestamps.Peek() < windowStart)
                {
                    callTimestamps.Dequeue();
                }

                if (callTimestamps.Count >= maxPerMinute)
                {
                    throw PDFGenerationThroughputExceededException.PerMinuteExceeded(maxPerMinute);
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