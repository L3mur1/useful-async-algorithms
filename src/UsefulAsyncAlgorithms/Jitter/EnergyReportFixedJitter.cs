namespace UsefulAsyncAlgorithms.Jitter
{
    /// <summary>
    /// Fixed jitter that prevents server overload by spreading energy reports over time.
    /// Each call waits for baseDelay plus a random amount up to maxJitter.
    /// </summary>
    /// <param name="baseDelay">Base wait time (always applied)</param>
    /// <param name="maxJitter">Maximum additional random delay</param>
    public class EnergyReportFixedJitter(TimeSpan baseDelay, TimeSpan maxJitter)
    {
        private readonly TimeSpan baseDelay = baseDelay;
        private readonly TimeSpan maxJitter = maxJitter;
        private readonly Random random = new();

        /// <summary>
        /// Waits for baseDelay + random jitter (0 to maxJitter) before sending.
        /// </summary>
        public async Task SendWithJitterAsync()
        {
            var jitterMilliseconds = random.Next(0, (int)maxJitter.TotalMilliseconds);
            var totalDelay = baseDelay.Add(TimeSpan.FromMilliseconds(jitterMilliseconds));

            await Task.Delay(totalDelay);

            // Sends report now
        }
    }
}