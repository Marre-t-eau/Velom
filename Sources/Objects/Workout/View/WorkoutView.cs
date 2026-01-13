using System.Collections.ObjectModel;

namespace Velom.Sources.Objects.Workout.View;

internal class WorkoutView : Workout
{
    internal ObservableCollection<WorkBlockView> BlocksView { get; } = [];

    internal WorkoutView(Workout workout): base(workout)
    {
        foreach (WorkBlock block in Blocks)
        {
            BlocksView.Add(new WorkBlockView(block));
        }
    }

    internal uint TotalDuration => (uint)Blocks.Sum(block => block.Duration);

    internal uint TotalDurationMinutes => (uint)(Blocks.Sum(block => block.Duration) / 60);

    internal int EstimatedTSS
    {
        get
        {
            if (FTP == 0 || Blocks.Count == 0)
                return 0;

            double totalWeightedPower = 0;
            uint totalDuration = 0;

            foreach (var block in Blocks)
            {
                if (block.TargetPowerStart.HasValue)
                {
                    // Average power for the block (considering start and end if it's a ramp)
                    double avgPower = block.TargetPowerEnd.HasValue 
                        ? (block.TargetPowerStart.Value + block.TargetPowerEnd.Value) / 2.0
                        : block.TargetPowerStart.Value;
                    
                    totalWeightedPower += avgPower * block.Duration;
                    totalDuration += block.Duration;
                }
            }

            if (totalDuration == 0)
                return 0;

            double normalizedPower = totalWeightedPower / totalDuration;
            double intensityFactor = normalizedPower / FTP;
            double tss = (totalDuration * normalizedPower * intensityFactor) / (FTP * 3600) * 100;

            return (int)Math.Round(tss);
        }
    }
}
