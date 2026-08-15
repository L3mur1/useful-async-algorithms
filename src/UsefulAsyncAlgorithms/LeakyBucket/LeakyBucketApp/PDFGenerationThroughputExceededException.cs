namespace LeakyBucketApp
{
    public class PDFGenerationThroughputExceededException : Exception
    {
        public PDFGenerationThroughputExceededException()
        {
        }

        public PDFGenerationThroughputExceededException(string? message) : base(message)
        {
        }

        public PDFGenerationThroughputExceededException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public static PDFGenerationThroughputExceededException PerSecondExceeded(int maxPerSecond)
            => new($"PDF generation throughput exceeded. Maximum {maxPerSecond} requests per second allowed.");
    }
}