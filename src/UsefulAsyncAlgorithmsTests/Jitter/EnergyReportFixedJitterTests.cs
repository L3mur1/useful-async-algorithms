using UsefulAsyncAlgorithms.Jitter;

namespace UsefulAsyncAlgorithmsTests.Jitter
{
    public class EnergyReportFixedJitterTests
    {
        [Fact]
        public async Task ShouldHaveDifferentCompletionTimes_WhenJitterApplied()
        {
            // Arrange
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitter = TimeSpan.FromMilliseconds(50);
            var jitterWithJitter = new EnergyReportFixedJitter(baseDelay, maxJitter);

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
            var maxJitter = TimeSpan.Zero;
            var jitterWithoutJitter = new EnergyReportFixedJitter(baseDelay, maxJitter);

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
    }
}