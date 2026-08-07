using LeakyBucketApp;

namespace LeakyBucketAppTests
{
    public class LeakyBucketTests
    {
        [Fact]
        public async Task ShouldThrottleWhenNoBucket()
        {
            await Assert.ThrowsAsync<PDFGenerationThroughputExceededException>(async () =>
            {
                await Task.CompletedTask;
            });
        }
    }
}