using Common;
using LeakyBucketApp;

namespace LeakyBucketAppTests
{
    public class LeakyBucketTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        [Fact]
        public async Task ShouldThrottleWhenNoBucket()
        {
            var testDuration = TimeSpan.FromSeconds(10);
            cts.CancelAfter(testDuration);

            await Assert.ThrowsAsync<PDFGenerationThroughputExceededException>(async () =>
            {
                var correspondanceRef = new CorrespondanceDocument();
                var steadyPublisher = new Publisher<CorrespondanceDocument>([correspondanceRef], TimeSpan.FromMilliseconds(300));
                await steadyPublisher.StartPublishingAsync(cts.Token);

                await Task.CompletedTask;
            });
        }
    }
}