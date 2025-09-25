using AchievementUploader.Models;

namespace AchievementUploader.Services;

public class AchievementUploadService
{
    private readonly SteamApiClient _steamClient;
    private readonly string _imagesFolder;
    private readonly int _permission; // 0 = Client, 1 = GameServer, 2 = "Official Game Server"

    public AchievementUploadService(SteamApiClient steamClient, string imagesFolder, int permission = 0)
    {
        _steamClient = steamClient;
        _imagesFolder = imagesFolder;
        _permission = permission;
    }

    public async Task<bool> ProcessAchievementsAsync(List<Achievement> achievements)
    {
        var existing = await _steamClient.FetchAchievementsAsync();

        Console.WriteLine($"Processing {achievements.Count} over existing {existing.Count} achievements...");
        
        foreach(var achievement in achievements)
        {
            Console.WriteLine($"\nProcessing achievement: {achievement.Id} - {achievement.DisplayName}");
            
            achievement.IconPath = Path.Combine(_imagesFolder, $"{achievement.Id}.jpg");
            achievement.UnachievedIconPath = Path.Combine(_imagesFolder, $"{achievement.Id}_unachieved.jpg");

            try
            {
                // Check if achievement already exists
                int statId, bitId;
                if (existing.TryGetValue(achievement.Id, out var existingAchievement))
                {
                    Console.WriteLine("  Achievement exists, updating...");
                    statId = existingAchievement.StatId;
                    bitId = existingAchievement.BitId ?? 0;
                }
                else
                {
                    Console.WriteLine("  Creating new achievement...");
                    (statId, bitId, _) = await _steamClient.CreateNewAchievementAsync();
                }

                // Save achievement data
                var saveResult = await _steamClient.SaveAchievementAsync(achievement, statId, bitId, _permission);
                if (!saveResult)
                {
                    Console.WriteLine($"  Failed to save achievement: {achievement.DisplayName}");
                }

                if (File.Exists(achievement.IconPath))
                {
                    // Upload achieved icon
                    var achievedUpload = await _steamClient.UploadImageAsync(achievement.IconPath, statId, bitId, false);
                    if (!achievedUpload.Success)
                    {
                        Console.WriteLine($"  Error uploading achieved icon: {achievedUpload.Error}");
                        //continue;
                    }

                    await Task.Delay(1000); // Seems images fail to upload if done too quickly, but still return success
                }
                else
                {
                    Console.WriteLine($"Warning: Achievement icon not found: {achievement.IconPath}");
                }

                if (File.Exists(achievement.UnachievedIconPath))
                {
                    // Upload unachieved icon
                    var unachievedUpload = await _steamClient.UploadImageAsync(achievement.UnachievedIconPath, statId, bitId, true);
                    if (!unachievedUpload.Success)
                    {
                        Console.WriteLine($"  Error uploading unachieved icon: {unachievedUpload.Error}");
                        //continue;
                    }

                    await Task.Delay(1000); // Seems images fail to upload if done too quickly, but still return success
                }
                else
                {
                    Console.WriteLine($"Warning: Unachieved icon not found: {achievement.UnachievedIconPath}");
                }

                // Add delay between requests to be respectful to the API
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error processing achievement {achievement.Id}: {ex}");
            }
        }

        Console.WriteLine("\nProcessing complete!");
        return true;
    }
}
