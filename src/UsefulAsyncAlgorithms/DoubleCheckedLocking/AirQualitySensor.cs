namespace UsefulAsyncAlgorithms.DoubleCheckedLocking
{
    public class AirQualitySensor
    {
        private int readCount = 0;
        public int ReadCount => readCount;

        public async Task<AirQualitySensorData> ReadSensorAsync()
        {
            var randomDelay = Random.Shared.Next(200, 800);
            await Task.Delay(randomDelay);

            var data = new AirQualitySensorData
            (
                PM25: Random.Shared.NextDouble() * 100,
                PM10: Random.Shared.NextDouble() * 100,
                Timestamp: DateTime.UtcNow
            );

            Interlocked.Increment(ref readCount);
            return data;
        }
    }
}