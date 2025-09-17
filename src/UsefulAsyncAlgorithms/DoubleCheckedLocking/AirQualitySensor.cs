namespace UsefulAsyncAlgorithms.DoubleCheckedLocking
{
    public class AirQualitySensor
    {
        public static async Task<AirQualitySensorData> ReadSensorAsync()
        {
            var randomDelay = Random.Shared.Next(200, 800);
            await Task.Delay(randomDelay);

            var data = new AirQualitySensorData
            (
                PM25: Random.Shared.NextDouble() * 100,
                PM10: Random.Shared.NextDouble() * 100,
                Timestamp: DateTime.UtcNow
            );

            return data;
        }
    }
}