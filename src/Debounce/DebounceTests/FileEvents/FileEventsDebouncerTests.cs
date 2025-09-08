using Debounce;
using Debounce.FileEvents;
using Xunit;

namespace DebounceTests.FileEvents
{
    public class FileEventsDebouncerTests
    {
        private readonly CancellationTokenSource cts;
        private readonly List<FileEvent> fileEvents;

        public FileEventsDebouncerTests()
        {
            cts = new CancellationTokenSource();

            fileEvents =
            [
                FileEvent.Create("file1.txt"),
                FileEvent.Create("file2.txt"),
                FileEvent.Create("file3.txt"),
                FileEvent.Create("file4.txt"),
                FileEvent.Create("file5.txt"),
                FileEvent.Create("file6.txt"),
            ];
        }

        [Fact]
        public async Task EventsStream_ShouldContainEventsWithSamePath_WhenOutOfWindow()
        {
            // Arrange
            var window = TimeSpan.FromMilliseconds(5);
            var eventsDelay = TimeSpan.FromMilliseconds(10);
            var testDuration = TimeSpan.FromSeconds(1);

            var publisher = new Publisher<FileEvent>(fileEvents);
            var debouncer = new FileEventsDebouncer(publisher, window);

            var receivedEvents = new List<FileEvent>();
            debouncer.FileEventStream.Subscribe(receivedEvents.Add);

            // Act
            cts.CancelAfter(testDuration);
            await publisher.StartPublishingAsync(eventsDelay, cts.Token);

            // Assert
            var samePathEvents = receivedEvents.GroupBy(e => e.Path);
            Assert.Contains(samePathEvents, g => g.Count() > 1);
        }

        [Fact()]
        public async Task EventsStream_ShouldNotContainEventsWithSamePath_WhenInWindow()
        {
            // Arrange
            var window = TimeSpan.FromSeconds(2);
            var eventsDelay = TimeSpan.FromMilliseconds(10);
            var testDuration = TimeSpan.FromSeconds(1);

            var publisher = new Publisher<FileEvent>(fileEvents);
            var debouncer = new FileEventsDebouncer(publisher, window);

            var receivedEvents = new List<FileEvent>();
            debouncer.FileEventStream.Subscribe(receivedEvents.Add);

            // Act
            cts.CancelAfter(testDuration);
            await publisher.StartPublishingAsync(eventsDelay, cts.Token);

            // Assert
            var samePathEvents = receivedEvents.GroupBy(e => e.Path);
            Assert.DoesNotContain(samePathEvents, g => g.Count() > 1);
        }
    }
}