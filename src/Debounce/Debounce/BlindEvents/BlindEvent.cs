namespace Debounce.BlindEvents
{
    public record BlindEvent(int Position, DateTime PublishTime) : IPublishable<BlindEvent>
    {
        public BlindEvent CreateNext() => this with { PublishTime = DateTime.UtcNow };
        public static BlindEvent Create(int position) => new(position, DateTime.MinValue);
    }
}