namespace UsefulAsyncAlgorithms
{
    public interface IPublishable<TPublishable>
    {
        TPublishable CreateNext();
    }
}