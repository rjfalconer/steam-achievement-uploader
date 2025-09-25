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
        Console.WriteLine($"Processing {achievements.Count} achievements...");
        
        for (int i = 0; i < achievements.Count; i++)
        {
            var achievement = achievements[i];
            int statId = i + 1;
            string bitId = "0"; // Unclear what this is
            
            Console.WriteLine($"\nProcessing achievement: {achievement.Id} - {achievement.DisplayName}");
            
            achievement.IconPath = Path.Combine(_imagesFolder, $"{achievement.Id}.jpg");
            achievement.UnachievedIconPath = Path.Combine(_imagesFolder, $"{achievement.Id}_unachieved.jpg");

            try
            {
                // Check if achievement already exists
                var existingAchievement = await _steamClient.FetchAchievementAsync(statId, bitId);
                bool isUpdate = existingAchievement != null;

                if (isUpdate)
                {
                    Console.WriteLine("  Achievement exists, updating...");
                }
                else
                {
                    Console.WriteLine("  Creating new achievement...");
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

                // Save achievement data
                var saveResult = await _steamClient.SaveAchievementAsync(achievement, statId, bitId, _permission);
                if (saveResult)
                {
                    Console.WriteLine($"  Successfully {(isUpdate ? "updated" : "created")}.");
                }
                else
                {
                    Console.WriteLine($"  Failed to save achievement: {achievement.DisplayName}");
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
