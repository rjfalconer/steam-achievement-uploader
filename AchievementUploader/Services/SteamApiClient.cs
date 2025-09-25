using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AchievementUploader.Models;

namespace AchievementUploader.Services;

public class SteamApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SteamSession _session;

    public SteamApiClient(SteamSession session)
    {
        _session = session;
        _httpClient = new HttpClient();

        SetupHttpClient();
    }

    private void SetupHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Origin", "https://partner.steamgames.com");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
        _httpClient.DefaultRequestHeaders.Add("Referer", $"https://partner.steamgames.com/apps/achievements/{_session.AppId}");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        
        var cookieValue = $"sessionid={_session.SessionId}; steamLoginSecure={Uri.EscapeDataString(_session.SteamLoginSecure)}";
        _httpClient.DefaultRequestHeaders.Add("Cookie", cookieValue);
    }

    public record AchievementSummary(int StatId, int BitId, string ApiName);
    record CreateAchievementResponse(AchievementSummary achievement, int success);
    public async Task<AchievementSummary> CreateNewAchievementAsync()
    {
        var url = $"https://partner.steamgames.com/apps/newachievement/{_session.AppId}";
        var content = new StringContent($"sessionid={_session.SessionId}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        options.AddNullFalseSupport();

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateAchievementResponse>(options);
        if (result!.success != 1) throw new Exception($"Failed to create new achievement {result}");
        return result.achievement;
    }

    record FetchAchievementsResponse(List<AchievementDetails> achievements);

    public async Task<IDictionary<string, AchievementDetails>> FetchAchievementsAsync()
    {
        var url = $"https://partner.steamgames.com/apps/fetchachievements/{_session.AppId}";
        var content = new StringContent($"sessionid={_session.SessionId}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        options.AddNullFalseSupport();

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FetchAchievementsResponse>(options);
        return result!.achievements.ToDictionary(a => a.ApiName);
    }

    public async Task<AchievementDetails?> FetchAchievementAsync(int statId, string bitId)
    {
        var url = $"https://partner.steamgames.com/apps/fetchachievement/{_session.AppId}/{statId}/{bitId}";
        var content = new StringContent($"sessionid={_session.SessionId}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "\"achievement not found\"")
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        options.AddNullFalseSupport();

        var result = JsonSerializer.Deserialize<AchievementDetails>(jsonString, options);
        return result;
    }

    public async Task<bool> SaveAchievementAsync(Achievement achievement, int statId = 1, int bitId = 0, int permission = 0)
    {
        var url = $"https://partner.steamgames.com/apps/saveachievement/{_session.AppId}";
        
        var displayNameObj = new { english = achievement.DisplayName, token = $"NEW_ACHIEVEMENT_{statId}_{bitId}_NAME" };
        var descriptionObj = new { english = achievement.Description, token = $"NEW_ACHIEVEMENT_{statId}_{bitId}_DESC" };
        
        var displayNameJson = JsonSerializer.Serialize(displayNameObj);
        var descriptionJson = JsonSerializer.Serialize(descriptionObj);
        
        var formData = new List<string>
        {
            $"statid={statId}",
            $"bitid={bitId}",
            $"apiname={Uri.EscapeDataString(achievement.Id)}",
            $"displayname={Uri.EscapeDataString(displayNameJson)}",
            $"description={Uri.EscapeDataString(descriptionJson)}",
            $"permission={permission}", // 0 = Client, 1 = GameServer, 2 = "Official Game Server"
            $"hidden={achievement.Hidden.ToString().ToLowerInvariant()}",
            $"progressStat={(string.IsNullOrWhiteSpace(achievement.LinkedStat) ? "-1" : achievement.LinkedStat)}",
            $"progressMin={achievement.MinStatValue}",
            $"progressMax={achievement.MaxStatValue}",
            $"sessionid={_session.SessionId}"
        };

        var content = new StringContent(string.Join("&", formData), Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        options.AddNullFalseSupport();

        var result = JsonSerializer.Deserialize<SteamAchievementResponse>(jsonString, options);
        if(result is { Success: 1, Saved: true }) return true;

        Console.WriteLine(jsonString);
        return false;
    }

    public async Task<ImageUploadResponse> UploadImageAsync(string imagePath, int statId, int bitId, bool isUnachieved = false)
    {
        var url = "https://partner.steamgames.com/images/uploadachievement";
        
        using var form = new MultipartFormDataContent();
        
        form.Add(new StringContent(_session.SessionId), "sessionid");
        form.Add(new StringContent(_session.AppId), "appID");
        form.Add(new StringContent(statId.ToString()), "statID");
        form.Add(new StringContent(bitId.ToString()), "bit");
        form.Add(new StringContent(isUnachieved ? "achievement_gray" : "achievement"), "requestType");

        // Add image file
        var imageBytes = await File.ReadAllBytesAsync(imagePath);
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(imageContent, "image", Path.GetFileName(imagePath));

        var response = await _httpClient.PostAsync(url, form);
        
        if (response.IsSuccessStatusCode)
        {
            return new ImageUploadResponse { Success = true };
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ImageUploadResponse { Success = false, Error = errorContent };
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
