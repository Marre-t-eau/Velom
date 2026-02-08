using Velom.Sources.Objects.Workout;

namespace Velom.Sources.Messages;

/// <summary>
/// Message sent when a workout is saved
/// </summary>
internal record WorkoutSavedMessage(Workout Workout);

/// <summary>
/// Message sent when a workout is deleted
/// </summary>
internal record WorkoutDeletedMessage(Guid WorkoutId);
