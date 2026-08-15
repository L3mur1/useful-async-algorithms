namespace LeakyBucketApp
{
    public class BucketOverflowException : Exception
    {
        public BucketOverflowException()
        {
        }

        public BucketOverflowException(string? message) : base(message)
        {
        }

        public BucketOverflowException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public static BucketOverflowException CapacityExceeded(int count, int capacity) =>
            new($"Leaky bucket capacity ({capacity}) exceeded with {count} items.");
    }
}