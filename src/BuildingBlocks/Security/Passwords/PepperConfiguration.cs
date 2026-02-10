namespace Bedrock.BuildingBlocks.Security.Passwords;

public class PepperConfiguration
{
    public int ActivePepperVersion { get; }
    public IReadOnlyDictionary<int, byte[]> Peppers { get; }

    public PepperConfiguration(
        int activePepperVersion,
        IReadOnlyDictionary<int, byte[]> peppers
    )
    {
        if (peppers is null || peppers.Count == 0)
            throw new ArgumentException("At least one pepper must be configured.", nameof(peppers));

        if (!peppers.ContainsKey(activePepperVersion))
            throw new ArgumentException(
                $"Active pepper version {activePepperVersion} is not in the peppers dictionary.",
                nameof(activePepperVersion));

        ActivePepperVersion = activePepperVersion;
        Peppers = peppers;
    }
}
