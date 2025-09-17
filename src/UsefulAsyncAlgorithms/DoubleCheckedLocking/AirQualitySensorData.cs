namespace UsefulAsyncAlgorithms.DoubleCheckedLocking
{
    public record AirQualitySensorData(double PM25, double PM10, DateTime Timestamp)
    {
        public bool IsValid(TimeSpan validityDuration) => DateTime.UtcNow - Timestamp <= validityDuration;
    }
}