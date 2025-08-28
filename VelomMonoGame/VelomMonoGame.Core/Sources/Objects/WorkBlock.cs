namespace VelomMonoGame.Core.Sources.Objects;

public class WorkBlock
{
    /// <summary>
    /// Duration in seconds
    /// </summary>
    public uint Duration { get; set; }
    /// <summary>
    /// Target power at start
    /// </summary>
    public ushort? TargetPowerStart { get; set; }
    /// <summary>
    /// Target power at end
    /// </summary>
    public ushort? TargetPowerEnd { get; set; }
    /// <summary>
    /// Target cadence
    /// </summary>
    public ushort? TargetCadence { get; set; }
    /// <summary>
    /// Type of power target
    /// </summary>
    public TargetPowerType PowerType { get; set; }

    public WorkBlock() { }
    public WorkBlock(WorkBlock workBlock)
    {
        Duration = workBlock.Duration;
        TargetPowerStart = workBlock.TargetPowerStart;
        TargetPowerEnd = workBlock.TargetPowerEnd;
        TargetCadence = workBlock.TargetCadence;
        PowerType = workBlock.PowerType;
    }
}

public enum TargetPowerType
{
    PercentFTP = 0,
    Watts = 1
}
