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

        public static PDFGenerationThroughputExceededException PerMinuteExceeded(int maxPerMinute)
            => new PDFGenerationThroughputExceededException($"PDF generation throughput exceeded. Maximum {maxPerMinute} requests per minute allowed.");
    }
}