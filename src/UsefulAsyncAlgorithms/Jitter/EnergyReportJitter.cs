namespace UsefulAsyncAlgorithms.Jitter
{
    /// <summary>
    /// Jitter implementation for energy consumption monitoring system.
    /// Distributes energy report sending over time to avoid server overload.
    /// </summary>
    public class EnergyReportJitter
    {
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan maxJitter;
        private readonly Random random = new();

        /// <summary>
        /// Creates a new instance of EnergyReportJitter
        /// </summary>
        /// <param name="baseDelay">Base delay before sending a report</param>
        /// <param name="maxJitter">Maximum random jitter added to base delay</param>
        public EnergyReportJitter(TimeSpan baseDelay, TimeSpan maxJitter)
        {
            _baseDelay = baseDelay;
            this.maxJitter = maxJitter;
        }

        /// <summary>
        /// Sends an energy report with jitter applied.
        /// Instead of sending all reports simultaneously, introduces random delay.
        /// </summary>
        /// <param name="report">Report to send</param>
        /// <returns>Task representing the asynchronous send operation</returns>
        public async Task SendWithJitterAsync(EnergyReport report)
        {
            // Calculate random delay: base delay + random jitter
            var jitterMilliseconds = random.Next(0, (int)maxJitter.TotalMilliseconds);
            var totalDelay = _baseDelay.Add(TimeSpan.FromMilliseconds(jitterMilliseconds));

            // Apply delay
            await Task.Delay(totalDelay);

            // Send report...
        }
    }
}