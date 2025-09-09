namespace Debounce.FileEvents
{
    public record FileEvent(string Path, DateTime PublishTime) : IPublishable<FileEvent>
    {
        public FileEvent CreateNext() => this with { PublishTime = DateTime.UtcNow };

        public static FileEvent Create(string path) => new(path, DateTime.MinValue);
    }
}