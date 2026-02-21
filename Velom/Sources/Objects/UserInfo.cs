

using System.Text.Json;

namespace Velom.Sources.Objects;

internal class UserInfo
{
    private ushort _FTP = 0;
    public ushort FTP
    {
        get
        {
            return _FTP;
        }
        set
        {
            if (_FTP == value)
                return;
            _FTP = value;
            SaveUserInfo();
        }
    }

    public UserInfo()
    {
    }

    public static async Task<UserInfo> GetUserInfo()
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, "UserInfo.json");

        if (File.Exists(filePath))
        {
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<UserInfo>(stream) ?? new UserInfo();
        }

        return new UserInfo();
    }

    private async void SaveUserInfo()
    {
        var userInfoJson = JsonSerializer.Serialize(this);
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "UserInfo.json");

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(userInfoJson);
    }
}
