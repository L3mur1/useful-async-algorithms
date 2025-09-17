using UsefulAsyncAlgorithms.DoubleCheckedLocking;

namespace UsefulAsyncAlgorithmsTests.DoubleCheckedLocking
{
    public class AQSensorDoubleCheckedLockerTests
    {
        [Fact]
        public async Task ShouldPerformNewRead_WhenDataIsExpired()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMilliseconds(100);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Act - First call to populate cache
            var firstData = await locker.GetCurrentDataAsync();

            // Wait for data to expire
            await Task.Delay(validityDuration + TimeSpan.FromMilliseconds(50));

            // Act - Second call should perform new read
            var secondData = await locker.GetCurrentDataAsync();

            // Assert
            Assert.NotSame(firstData, secondData);
            Assert.True(secondData.Timestamp > firstData.Timestamp);
        }

        [Fact]
        public async Task ShouldPreventConcurrentReads_WhenMultipleCallersRequestExpiredData()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMilliseconds(50);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Populate cache first
            await locker.GetCurrentDataAsync();
            var initialReadCount = sensor.ReadCount;

            // Wait for data to expire
            await Task.Delay(validityDuration + TimeSpan.FromMilliseconds(50));

            // Act - Multiple concurrent calls
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => locker.GetCurrentDataAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert - Only one additional sensor read should have occurred
            Assert.Equal(initialReadCount + 1, sensor.ReadCount);

            // All results should be the same (same cached data)
            var firstResult = results[0];
            Assert.All(results, result =>
            {
                Assert.Same(firstResult, result);
                Assert.Equal(firstResult.Timestamp, result.Timestamp);
            });
        }

        [Fact]
        public async Task ShouldReturnSameData_WhenConcurrentCallsOnValidData()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMinutes(5);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Act - Multiple concurrent calls on valid data
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => locker.GetCurrentDataAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert - Only one sensor read should have occurred
            Assert.Equal(1, sensor.ReadCount);

            // All results should be the same
            var firstResult = results[0];
            Assert.All(results, result =>
            {
                Assert.Same(firstResult, result);
                Assert.Equal(firstResult.Timestamp, result.Timestamp);
            });
        }
    }
}