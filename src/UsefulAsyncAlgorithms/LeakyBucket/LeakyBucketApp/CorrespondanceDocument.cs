using Common;

namespace LeakyBucketApp
{
    public record CorrespondanceDocument : IPublishable<CorrespondanceDocument>
    {
        public CorrespondanceDocument Next() => this;
    }
}