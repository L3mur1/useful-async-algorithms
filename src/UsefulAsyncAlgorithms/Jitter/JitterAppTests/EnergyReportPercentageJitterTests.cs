using JitterApp;

namespace JitterAppTests
{
    public class EnergyReportPercentageJitterTests
    {
        [Fact]
        public void CalculateDelay_ShouldEqualBaseDelay_WhenZeroJitter()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var jitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage: 0);

            for (var i = 0; i < 20; i++)
            {
                Assert.Equal(baseDelay, jitter.CalculateDelay());
            }
        }

        [Fact]
        public void CalculateDelay_ShouldStayWithinRange_WhenJitterApplied()
        {
            var baseDelay = TimeSpan.FromMilliseconds(200);
            var maxJitterPercentage = 25;
            var jitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage);
            var maxDelay = baseDelay + TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * maxJitterPercentage / 100.0);

            for (var i = 0; i < 100; i++)
            {
                var delay = jitter.CalculateDelay();
                Assert.InRange(delay, baseDelay, maxDelay);
            }
        }

        [Fact]
        public void CalculateDelay_ShouldUseInjectedRandom()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxJitterPercentage = 50;
            var jitter = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage, random: new Random(42));

            var first = jitter.CalculateDelay();
            var second = new EnergyReportPercentageJitter(baseDelay, maxJitterPercentage, random: new Random(42)).CalculateDelay();

            Assert.Equal(first, second);
            Assert.InRange(
                first,
                baseDelay,
                baseDelay + TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * maxJitterPercentage / 100.0));
        }
    }
}
