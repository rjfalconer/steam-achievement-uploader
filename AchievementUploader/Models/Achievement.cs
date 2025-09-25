using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
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
    public int? BitId { get; set; } // Sometimes string, sometimes int
    [JsonPropertyName("api_name")]
    public string ApiName { get; set; } = string.Empty;
    [JsonPropertyName("display_name")]
    public DisplayNameObject DisplayName { get; set; } = new();
    [JsonPropertyName("description")]
    public DescriptionObject Description { get; set; } = new();
    [JsonPropertyName("permission")]
    public int Permission { get; set; }
    [JsonPropertyName("hidden")]
    public string Hidden { get; set; } = "0";
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    [JsonPropertyName("icon_gray")]
    public string IconGrey { get; set; } = string.Empty;
    [JsonPropertyName("progress")]
    public NullAsFalse<ProgressDetails> ProgressDetails { get; set; }
}

public class ProgressDetails
{
    [JsonPropertyName("min_val")]
    public int MinVal { get; set; }
    [JsonPropertyName("max_val")]
    public int MaxVal { get; set; }
    [JsonPropertyName("value")]
    public ProgressDetailsStat Stat { get; set; } = new();
}

public class ProgressDetailsStat
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = "statvalue";
    [JsonPropertyName("operand1")]
    public string Stat { get; set; }
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
