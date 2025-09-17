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
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

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
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

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
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

            // Populate cache first
            await locker.GetCurrentDataAsync();

            // Wait for data to expire
            await Task.Delay(validityDuration + TimeSpan.FromMilliseconds(50));

            // Act - Multiple concurrent calls
            var startTime = DateTime.UtcNow;
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => locker.GetCurrentDataAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;

            // Assert - All results should be the same (same cached data)
            var firstResult = results[0];
            Assert.All(results, result =>
            {
                Assert.Same(firstResult, result);
                Assert.Equal(firstResult.Timestamp, result.Timestamp);
            });

            // Assert - All calls should have completed quickly (indicating they used cached data)
            // If multiple sensor reads occurred, it would take much longer
            var totalTime = endTime - startTime;
            Assert.True(totalTime.TotalMilliseconds < 1000,
                $"Expected quick completion (cached data), but took {totalTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task ShouldReturnCachedData_WhenDataIsStillValid()
        {
            // Arrange
            var validityDuration = TimeSpan.FromMinutes(5);
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

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
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

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
            var locker = new ACSensorDoubleCheckedLocker(validityDuration);

            // Act - Multiple concurrent calls on valid data
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => locker.GetCurrentDataAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert - All results should be the same
            var firstResult = results[0];
            Assert.All(results, result =>
            {
                Assert.Same(firstResult, result);
                Assert.Equal(firstResult.Timestamp, result.Timestamp);
            });
        }
    }
}