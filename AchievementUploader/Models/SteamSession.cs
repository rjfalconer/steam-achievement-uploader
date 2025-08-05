namespace AchievementUploader.Models;

public class SteamSession
{
    public string SessionId { get; set; } = string.Empty;
    public string SteamLoginSecure { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
}

public class ImageUploadResponse
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }
}
