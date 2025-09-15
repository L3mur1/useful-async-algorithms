namespace UsefulAsyncAlgorithms.Jitter
{
    public record EnergyReport(string Device, double UsedEnergy) : IPublishable<EnergyReport>
    {
        public EnergyReport CreateNext()
        {
            var deviceName = $"device_{Random.Shared.Next()}";
            var usedEnergy = Random.Shared.NextDouble() * 100;

            return new EnergyReport(deviceName, usedEnergy);
        }
    }
}