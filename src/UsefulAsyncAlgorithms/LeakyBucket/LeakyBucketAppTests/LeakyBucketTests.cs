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
            var publishingTime = TimeSpan.FromSeconds(10);
            cts.CancelAfter(publishingTime);
        }

        [Fact]
        public async Task ShouldNotThrottle_WhenUsingLeakyBucket()
        {
            // Arrange
            var correspondenceRef = new CorrespondenceDocument();
            var bucket = new CorrespondenceLeakyBucket(leakInterval: TimeSpan.FromMilliseconds(100), capacity: 100);

            var steadyPublisher = new Publisher<CorrespondenceDocument>([correspondenceRef], tickDelay: TimeSpan.FromMilliseconds(250));
            steadyPublisher.MessageStream.Subscribe(bucket.AddToBucket);

            var burstPublisher = new Publisher<CorrespondenceDocument>([correspondenceRef], tickDelay: TimeSpan.FromSeconds(2), batchSize: 8);
            burstPublisher.MessageStream.Subscribe(bucket.AddToBucket);

            bucket.LeakyStream.Subscribe(doc => pdfGenerator.GeneratePDF(doc));

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                var leaking = bucket.StartLeakingAsync();

                await Task.WhenAll(
                    steadyPublisher.StartPublishingAsync(cts.Token),
                    burstPublisher.StartPublishingAsync(cts.Token));

                bucket.Complete();
                await leaking;
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldThrottle_WhenNoBucket()
        {
            await Assert.ThrowsAsync<PDFGenerationThroughputExceededException>(async () =>
            {
                // Arrange
                var correspondenceRef = new CorrespondenceDocument();

                var steadyPublisher = new Publisher<CorrespondenceDocument>([correspondenceRef], tickDelay: TimeSpan.FromMilliseconds(250));
                steadyPublisher.MessageStream.Subscribe(doc =>
                {
                    pdfGenerator.GeneratePDF(doc, cts.Token);
                });

                var burstPublisher = new Publisher<CorrespondenceDocument>([correspondenceRef], tickDelay: TimeSpan.FromSeconds(2), batchSize: 8);
                burstPublisher.MessageStream.Subscribe(doc =>
                {
                    pdfGenerator.GeneratePDF(doc, cts.Token);
                });

                var publishing = await Task.WhenAny(
                    steadyPublisher.StartPublishingAsync(cts.Token),
                    burstPublisher.StartPublishingAsync(cts.Token));

                cts.Cancel();
                await publishing;
            });
        }
    }
}