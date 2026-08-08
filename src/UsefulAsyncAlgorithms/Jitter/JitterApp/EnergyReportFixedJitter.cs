namespace JitterApp
{
    /// <summary>
    /// Fixed jitter that prevents server overload by spreading energy reports over time.
    /// Each call waits for baseDelay plus a random amount up to maxJitter.
    /// </summary>
    /// <param name="baseDelay">Base wait time (always applied)</param>
    /// <param name="maxJitter">Maximum additional random delay</param>
    public class EnergyReportFixedJitter(TimeSpan baseDelay, TimeSpan maxJitter, Random? random = null)
    {
        private readonly TimeSpan baseDelay = baseDelay;
        private readonly TimeSpan maxJitter = maxJitter;
        private readonly Random random = random ?? new();

        /// <summary>
        /// Computes baseDelay + random jitter in [0, maxJitter].
        /// </summary>
        public TimeSpan CalculateDelay()
        {
            if (maxJitter <= TimeSpan.Zero)
            {
                return baseDelay;
            }

            var jitterMilliseconds = random.Next(0, (int)maxJitter.TotalMilliseconds + 1);
            return baseDelay.Add(TimeSpan.FromMilliseconds(jitterMilliseconds));
        }

        /// <summary>
        /// Waits for <see cref="CalculateDelay"/> before sending.
        /// </summary>
        public async Task SendWithJitterAsync()
        {
            await Task.Delay(CalculateDelay());

            // Sends report now
        }
    }
}
