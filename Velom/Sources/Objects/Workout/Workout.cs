using System.Text.Json.Serialization;

namespace Velom.Sources.Objects.Workout;

internal class Workout
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<WorkBlock> Blocks { get; set; } = new List<WorkBlock>();
    public string Name { get; set; } = string.Empty;

    public Workout() { }
    internal Workout(Workout workout)
    {
        Name = workout.Name;
        Id = workout.Id;
        _FTP = workout._FTP;
        
        foreach(WorkBlock workBlock in workout.Blocks)
        {
            Blocks.Add(new WorkBlock(workBlock));
        }
    }

    private ushort _FTP;
    [JsonIgnore]
    internal ushort FTP
    {
        get
        {
            return _FTP;
        }
        set
        {
            _FTP = value;
            // Set blocks power accordingly
            foreach (WorkBlock workBlock in Blocks)
            {
                if (workBlock.PowerType == WorkBlock.TargetPowerType.PercentFTP)
                {
                    if (workBlock.TargetPowerStart != null)
                        workBlock.TargetPowerStart = TransformPower(value, workBlock.TargetPowerStart.Value);
                    if (workBlock.TargetPowerEnd != null)
                        workBlock.TargetPowerEnd = TransformPower(value, workBlock.TargetPowerEnd.Value);
                }
            }
        }
    }

    private ushort TransformPower(ushort FTP, ushort percentageOfPower)
    {
        double percent = percentageOfPower * .01;
        ushort result = (ushort)(FTP * percent);
        // We take the upper 5
        return ToUpper5(result);
    }

    internal static ushort ToUpper5(ushort power)
    {
        int rest = power % 5;
        if (rest == 0)
            return power;
        return (ushort)(power + (5 - rest));
    }
}
