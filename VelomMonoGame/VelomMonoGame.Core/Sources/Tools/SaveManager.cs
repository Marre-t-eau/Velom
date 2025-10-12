using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelomMonoGame.Core.Sources.Objects;

namespace VelomMonoGame.Core.Sources.Tools;

internal static class SaveManager
{
    private const string FolderName = "Velom";
    private const string WorkoutFolderName = "Workouts";
    private const string GameLogsFolderName = "GameLogs";
    private const string UserDataFileName = "user_data.json";

    private static string GetSaveDirectory(string subFolder = null)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string saveDir;
        if (subFolder == null)
            saveDir = Path.Combine(appData, FolderName);
        else
            saveDir = Path.Combine(appData, FolderName, subFolder);
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        return saveDir;
    }

    public static UserData LoadUserData()
    {
        string filePath = Path.Combine(GetSaveDirectory(), UserDataFileName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<UserData>(json);
        }
        return new UserData();
    }

    public static void SaveUserData(UserData data)
    {
        string filePath = Path.Combine(GetSaveDirectory(), UserDataFileName);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static Workout GetWorkout(string workoutFileName)
    {
        string filePath = Path.Combine(GetSaveDirectory(WorkoutFolderName), $"{workoutFileName}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Workout>(json);
        }
        throw new FileNotFoundException("Workout file not found.", filePath);
    }

    public static void SaveWorkout(Workout workout, string workoutFileName = null)
    {
        if (string.IsNullOrEmpty(workoutFileName))
        {
            // Generate filename from workout name
            int hash = workout.GetHashCode();
            while (GetAllWorkoutFiles().Contains(hash.ToString()))
            {
                hash++;
            }
            workoutFileName = hash.ToString();
        }
        string filePath = Path.Combine(GetSaveDirectory(WorkoutFolderName), $"{workoutFileName}.json");
        string json = JsonSerializer.Serialize(workout, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static List<string> GetAllWorkoutFiles()
    {
        string workoutDir = GetSaveDirectory(WorkoutFolderName);
        if (Directory.Exists(workoutDir))
        {
            var files = Directory.GetFiles(workoutDir, "*.json");
            List<string> workoutNames = new List<string>();
            // Strip directory and extension
            for (int i = 0; i < files.Length; i++)
            {
                workoutNames.Add(Path.GetFileNameWithoutExtension(files[i]));
            }
            return workoutNames;
        }
        return new List<string>();
    }

    public static void DeleteWorkout(string workoutFileName)
    {
        string filePath = Path.Combine(GetSaveDirectory(WorkoutFolderName), $"{workoutFileName}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void SaveGameLog(IEnumerable<GameLogEntry> logs)
    {
        // Nom de fichier unique basé sur la date et l'heure
        string fileName = $"GameLog_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(GetSaveDirectory(GameLogsFolderName), fileName);

        string json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
