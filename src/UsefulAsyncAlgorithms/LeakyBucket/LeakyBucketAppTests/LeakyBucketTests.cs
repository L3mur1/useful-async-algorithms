using Common;
using LeakyBucketApp;

namespace LeakyBucketAppTests
{
    public class LeakyBucketTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly PDFGenerator pdfGenerator = new PDFGenerator(maxPerSecond: 10);

        public LeakyBucketTests()
        {
            var maxTestDuration = TimeSpan.FromSeconds(10);
            cts.CancelAfter(maxTestDuration);
        }

        [Fact]
        public async Task ShouldThrottleWhenNoBucket()
        {
            await Assert.ThrowsAsync<PDFGenerationThroughputExceededException>(async () =>
            {
                // Arrange
                var correspondanceRef = new CorrespondanceDocument();

                var steadyPublisher = new Publisher<CorrespondanceDocument>([correspondanceRef], tickDelay: TimeSpan.FromMilliseconds(250));
                steadyPublisher.MessageStream.Subscribe(doc =>
                {
                    pdfGenerator.GeneratePDF(doc, cts.Token);
                });

                var burstPublisher = new Publisher<CorrespondanceDocument>([correspondanceRef], tickDelay: TimeSpan.FromSeconds(2), batchSize: 8);
                burstPublisher.MessageStream.Subscribe(doc =>
                {
                    pdfGenerator.GeneratePDF(doc, cts.Token);
                });

                // Act
                List<Task> tasks =
                [
                    steadyPublisher.StartPublishingAsync(cts.Token),
                    burstPublisher.StartPublishingAsync(cts.Token),
                ];

                await Task.WhenAll(tasks);
            });
        }
    }
}