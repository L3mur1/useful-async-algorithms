namespace UsefulAsyncAlgorithms.DoubleCheckedLocking
{
    public class ACSensorDoubleCheckedLocker(TimeSpan validityDuration) : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private AirQualitySensorData? cachedData;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets current air quality data, performing expensive sensor read only if necessary.
        /// Uses double-checked locking with SemaphoreSlim to prevent multiple concurrent expensive operations.
        /// </summary>
        public async Task<AirQualitySensorData> GetCurrentDataAsync()
        {
            // First check: Is cached data still valid? (no lock needed for read)
            if (cachedData?.IsValid(validityDuration) == true)
            {
                return cachedData;
            }

            // Acquire semaphore to ensure only one thread can proceed to expensive operation
            await semaphore.WaitAsync();
            try
            {
                // Second check: Re-verify data validity after acquiring semaphore
                if (cachedData?.IsValid(validityDuration) == true)
                {
                    return cachedData;
                }

                // Perform the expensive sensor read operation and update cache
                cachedData = await AirQualitySensor.ReadSensorAsync();

                return cachedData;
            }
            finally
            {
                // Ensure semaphore is released even if an exception occurs
                semaphore.Release();
            }
        }

        protected virtual void Dispose(bool disposing) => semaphore?.Dispose();
    }
}