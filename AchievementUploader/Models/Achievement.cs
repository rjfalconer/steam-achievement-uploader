using System.Text.Json.Serialization;

namespace AchievementUploader.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public string? IconPath { get; set; }
    public string? UnachievedIconPath { get; set; }
}

public class SteamAchievementResponse
{
    public int Success { get; set; }
    public bool Saved { get; set; }
    public AchievementDetails? Achievement { get; set; }
}

public class AchievementDetails
{
    [JsonPropertyName("stat_id")]
    public int StatId { get; set; }
    [JsonPropertyName("bit_id")]
    public dynamic? BitId { get; set; } // Sometimes string, sometimes int
    [JsonPropertyName("api_name")]
    public string ApiName { get; set; } = string.Empty;
    [JsonPropertyName("display_name")]
    public DisplayNameObject DisplayName { get; set; } = new();
    [JsonPropertyName("description")]
    public DescriptionObject Description { get; set; } = new();
    [JsonPropertyName("permission")]
    public string Permission { get; set; }
    [JsonPropertyName("hidden")]
    public string Hidden { get; set; } = "0";
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    [JsonPropertyName("icon_gray")]
    public string IconGrey { get; set; } = string.Empty;
    [JsonPropertyName("progress")]
    public bool Progress { get; set; }
}

public class DisplayNameObject
{
    [JsonPropertyName("english")]
    public string English { get; set; } = string.Empty;
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}

public class DescriptionObject
{
    [JsonPropertyName("english")]
    public string English { get; set; } = string.Empty;
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
