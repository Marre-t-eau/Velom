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
}
