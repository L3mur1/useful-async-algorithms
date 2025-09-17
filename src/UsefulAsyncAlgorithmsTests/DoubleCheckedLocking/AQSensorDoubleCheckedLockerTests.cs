using UsefulAsyncAlgorithms.DoubleCheckedLocking;

namespace UsefulAsyncAlgorithmsTests.DoubleCheckedLocking
{
    public class AQSensorDoubleCheckedLockerTests
    {
        [Fact]
        public async Task ShouldHandleExceptions_WithoutCorruptingState()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMinutes(5);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Act & Assert - First call should work
            var firstData = await locker.GetCurrentDataAsync();
            Assert.NotNull(firstData);

            // Act & Assert - Subsequent calls should still work
            var secondData = await locker.GetCurrentDataAsync();
            Assert.Same(firstData, secondData);
        }

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
        public async Task ShouldReturnCachedData_WhenDataIsStillValid()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMinutes(5);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Act - First call to populate cache
            var firstData = await locker.GetCurrentDataAsync();

            // Act - Second call should return cached data
            var secondData = await locker.GetCurrentDataAsync();

            // Assert
            Assert.Same(firstData, secondData);
            Assert.Equal(firstData.Timestamp, secondData.Timestamp);
        }

        [Fact]
        public async Task ShouldReturnDifferentData_AfterCacheExpiration()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMilliseconds(200);
            var sensor = new AirQualitySensor();
            var locker = new AQSensorDoubleCheckedLocker(sensor, validityDuration);

            // Act - Get initial data
            var initialData = await locker.GetCurrentDataAsync();

            // Wait for expiration
            await Task.Delay(validityDuration + TimeSpan.FromMilliseconds(100));

            // Act - Get new data
            var newData = await locker.GetCurrentDataAsync();

            // Assert
            Assert.NotSame(initialData, newData);
            Assert.True(newData.Timestamp > initialData.Timestamp);
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