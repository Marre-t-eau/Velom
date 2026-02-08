using System.Text.Json;
using Velom.Sources.Objects.Workout;

namespace Velom.Sources.Services;

internal class WorkoutStorageService
{
    private static readonly string WorkoutsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Workouts");
    private static readonly string WorkoutsFile = Path.Combine(WorkoutsDirectory, "workouts.json");

    static WorkoutStorageService()
    {
        // Ensure directory exists
        if (!Directory.Exists(WorkoutsDirectory))
        {
            Directory.CreateDirectory(WorkoutsDirectory);
        }
    }

    public static async Task<List<Workout>> LoadWorkoutsAsync()
    {
        try
        {
            if (!File.Exists(WorkoutsFile))
            {
                return new List<Workout>();
            }

            string json = await File.ReadAllTextAsync(WorkoutsFile);
            var workouts = JsonSerializer.Deserialize<List<Workout>>(json);
            return workouts ?? new List<Workout>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading workouts: {ex.Message}");
            return new List<Workout>();
        }
    }

    public static async Task SaveWorkoutsAsync(List<Workout> workouts)
    {
        try
        {
            string json = JsonSerializer.Serialize(workouts);
            await File.WriteAllTextAsync(WorkoutsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving custom workouts: {ex.Message}");
        }
    }

    /// <summary>
    /// Save or update a single workout by ID
    /// </summary>
    public static async Task SaveWorkoutAsync(Workout workout)
    {
        try
        {
            var allWorkouts = await LoadWorkoutsAsync();

            // Find existing workout by ID
            int existingIndex = allWorkouts.FindIndex(w => w.Id == workout.Id);

            if (existingIndex >= 0)
            {
                // Update existing workout
                allWorkouts[existingIndex] = workout;
            }
            else
            {
                // Add new workout
                allWorkouts.Add(workout);
            }

            await SaveWorkoutsAsync(allWorkouts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving workout: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a workout by ID
    /// </summary>
    public static async Task<bool> DeleteWorkoutAsync(Guid workoutId)
    {
        try
        {
            var allWorkouts = await LoadWorkoutsAsync();
            int removedCount = allWorkouts.RemoveAll(w => w.Id == workoutId);

            if (removedCount > 0)
            {
                await SaveWorkoutsAsync(allWorkouts);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting workout: {ex.Message}");
            return false;
        }
    }
}
