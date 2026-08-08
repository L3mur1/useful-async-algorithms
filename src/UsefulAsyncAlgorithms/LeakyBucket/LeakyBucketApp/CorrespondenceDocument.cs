using Common;

namespace LeakyBucketApp
{
    public record CorrespondenceDocument : IPublishable<CorrespondenceDocument>
    {
        public CorrespondenceDocument Next() => this;
    }
}
