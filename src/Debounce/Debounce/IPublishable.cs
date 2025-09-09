namespace Debounce
{
    public interface IPublishable<TPublishable>
    {
        TPublishable CreateNext();
    }
}