using UsefulAsyncAlgorithms.Jitter;

namespace UsefulAsyncAlgorithmsTests.Jitter
{
    public class EnergyReportPercentageJitterTests
    {
        [Fact]
        public async Task ShouldHaveDifferentCompletionTimes_WhenJitterApplied()
        {
            // Arrange
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitterPercentage = 50; // 50% of baseDelay
            var jitterWithJitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage);

            // Act - Execute calls sequentially to avoid system load interference
            var startTime1 = DateTime.UtcNow;
            await jitterWithJitter.SendWithJitterAsync();
            var endTime1 = DateTime.UtcNow;

            var startTime2 = DateTime.UtcNow;
            await jitterWithJitter.SendWithJitterAsync();
            var endTime2 = DateTime.UtcNow;

            var duration1 = endTime1 - startTime1;
            var duration2 = endTime2 - startTime2;

            // Assert
            var timeDifference = Math.Abs(duration1.TotalMilliseconds - duration2.TotalMilliseconds);
            Assert.True(timeDifference > 5, $"Expected different completion times, but got similar durations: {duration1.TotalMilliseconds:F2}ms vs {duration2.TotalMilliseconds:F2}ms");
        }

        [Fact]
        public async Task ShouldHaveSameCompletionTimes_WhenZeroJitter()
        {
            // Arrange
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitterPercentage = 0; // No jitter
            var jitterWithoutJitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage);

            // Act - Execute calls sequentially to avoid system load interference
            var startTime1 = DateTime.UtcNow;
            await jitterWithoutJitter.SendWithJitterAsync();
            var endTime1 = DateTime.UtcNow;

            var startTime2 = DateTime.UtcNow;
            await jitterWithoutJitter.SendWithJitterAsync();
            var endTime2 = DateTime.UtcNow;

            var duration1 = endTime1 - startTime1;
            var duration2 = endTime2 - startTime2;

            // Assert
            var timeDifference = Math.Abs(duration1.TotalMilliseconds - duration2.TotalMilliseconds);
            Assert.True(timeDifference <= 10, $"Expected similar completion times with zero jitter, but got different durations: {duration1.TotalMilliseconds:F2}ms vs {duration2.TotalMilliseconds:F2}ms");
        }

        [Fact]
        public async Task SendWithJitterAsync_ShouldRespectPercentageJitterRange()
        {
            // Arrange
            var baseDelay = TimeSpan.FromMilliseconds(200);
            var maxJitterPercentage = 25; // 25% of baseDelay = max 50ms additional
            var jitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage);

            // Act
            var startTime = DateTime.UtcNow;
            await jitter.SendWithJitterAsync();
            var endTime = DateTime.UtcNow;

            var actualDuration = endTime - startTime;

            // Assert - Duration should be between baseDelay and baseDelay + 25% of baseDelay
            var minExpectedDuration = baseDelay.TotalMilliseconds;
            var maxExpectedDuration = baseDelay.TotalMilliseconds * 1.25; // 100% + 25%

            Assert.True(actualDuration.TotalMilliseconds >= minExpectedDuration - 10,
                $"Duration {actualDuration.TotalMilliseconds:F2}ms should be at least {minExpectedDuration}ms (base delay)");

            Assert.True(actualDuration.TotalMilliseconds <= maxExpectedDuration + 10,
                $"Duration {actualDuration.TotalMilliseconds:F2}ms should be at most {maxExpectedDuration}ms (base delay + 25%)");
        }
    }
}