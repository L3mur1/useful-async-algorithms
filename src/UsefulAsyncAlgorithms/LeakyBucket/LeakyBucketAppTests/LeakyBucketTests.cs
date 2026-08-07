using Common;
using LeakyBucketApp;

namespace LeakyBucketAppTests
{
    public class LeakyBucketTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly PDFGenerator pdfGenerator = new PDFGenerator(maxPerSecond: 10);

        [Fact]
        public async Task ShouldThrottleWhenNoBucket()
        {
            var testDuration = TimeSpan.FromSeconds(10);
            cts.CancelAfter(testDuration);

            await Assert.ThrowsAsync<PDFGenerationThroughputExceededException>(async () =>
            {
                var correspondanceRef = new CorrespondanceDocument();
                var steadyPublisher = new Publisher<CorrespondanceDocument>([correspondanceRef], tickDelay: TimeSpan.FromMilliseconds(300));
                var sub = steadyPublisher.MessageStream.Subscribe(async doc =>
                {
                    await pdfGenerator.GeneratePDFAsync(doc, cts.Token);
                });

                await steadyPublisher.StartPublishingAsync(cts.Token);

                await Task.CompletedTask;
            });
        }
    }
}