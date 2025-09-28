using System;
using System.IO;
using System.Text.Json;
using VelomMonoGame.Core.Sources.Objects;

namespace VelomMonoGame.Core.Sources.Tools;

internal static class SaveManager
{
    private const string FolderName = "Velom";
    private const string UserDataFileName = "user_data.json";

    private static string GetSaveDirectory()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string saveDir = Path.Combine(appData, FolderName);
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
}
