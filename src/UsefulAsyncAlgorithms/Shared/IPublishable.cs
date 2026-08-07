namespace Common
{
    public interface IPublishable<TPublishable>
    {
        TPublishable Next();
    }
}