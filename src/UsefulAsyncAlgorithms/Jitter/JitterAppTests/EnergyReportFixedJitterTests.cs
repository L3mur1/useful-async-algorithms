using JitterApp;

namespace JitterAppTests
{
    public class EnergyReportFixedJitterTests
    {
        [Fact]
        public void CalculateDelay_ShouldEqualBaseDelay_WhenZeroJitter()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var jitter = new EnergyReportFixedJitter(baseDelay, maxJitter: TimeSpan.Zero);

            for (var i = 0; i < 20; i++)
            {
                Assert.Equal(baseDelay, jitter.CalculateDelay());
            }
        }

        [Fact]
        public void CalculateDelay_ShouldStayWithinRange_WhenJitterApplied()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitter = TimeSpan.FromMilliseconds(50);
            var jitter = new EnergyReportFixedJitter(baseDelay, maxJitter);

            for (var i = 0; i < 100; i++)
            {
                var delay = jitter.CalculateDelay();
                Assert.InRange(delay, baseDelay, baseDelay + maxJitter);
            }
        }

        [Fact]
        public void CalculateDelay_ShouldUseInjectedRandom()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitter = TimeSpan.FromMilliseconds(50);
            var jitter = new EnergyReportFixedJitter(baseDelay, maxJitter, random: new Random(42));

            var first = jitter.CalculateDelay();
            var second = new EnergyReportFixedJitter(baseDelay, maxJitter, random: new Random(42)).CalculateDelay();

            Assert.Equal(first, second);
            Assert.InRange(first, baseDelay, baseDelay + maxJitter);
        }
    }
}
