namespace UsefulAsyncAlgorithms.Jitter
{
    /// <summary>
    /// Percentage jitter that prevents server overload by spreading energy reports over time.
    /// Each call waits for baseDelay plus a random percentage of baseDelay.
    /// </summary>
    /// <param name="baseDelay">Base wait time (always applied)</param>
    /// <param name="maxJitterPercentage">Maximum random percentage of baseDelay to add (0-100)</param>
    public class EnergyReportPercentageJitter(TimeSpan baseDelay, int maxJitterPercentage)
    {
        private readonly TimeSpan baseDelay = baseDelay;
        private readonly int maxJitterPercentage = maxJitterPercentage;
        private readonly Random random = new();

        /// <summary>
        /// Waits for baseDelay + random percentage jitter before sending.
        /// </summary>
        public async Task SendWithJitterAsync()
        {
            var jitterPercentage = random.Next(0, maxJitterPercentage + 1);
            var jitterMilliseconds = (int)(baseDelay.TotalMilliseconds * jitterPercentage / 100.0);
            var totalDelay = baseDelay.Add(TimeSpan.FromMilliseconds(jitterMilliseconds));

            await Task.Delay(totalDelay);

            // Sends report now
        }
    }
}
