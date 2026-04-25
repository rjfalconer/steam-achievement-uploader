using System.CommandLine;
using System.CommandLine.Parsing;
using AchievementUploader.Models;
using AchievementUploader.Services;

namespace AchievementUploader;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var csvFileOption = new Option<string>("--csv")
        {
            Description = "Path to the CSV file containing achievement data",
            Required = true
        };

        var imagesDirectoryOption = new Option<string?>("--images")
        {
            Description = "Path to the directory containing achievement images",
        };

        var sessionIdOption = new Option<string>("--session-id")
        {
            Description = "Steam session ID",
            Required = true
        };

        var steamLoginSecureOption = new Option<string>("--steam-login-secure")
        {
            Description = "Steam login secure cookie value",
            Required = true
        };

        var appIdOption = new Option<string>("--app-id")
        {
            Description = "Steam App ID",
            Required = true
        };

        var generateImagesOption = new Option<bool>("--generate-images")
        {
            Description = "Generate greyscale _unachieved versions of achievement images before uploading",
        };

        var permissionOption = new Option<int>("--permission")
        {
            Description = "Permission level for achievements (0 = Client, 1 = GameServer, 2 = Official Game Server)",
            DefaultValueFactory = _ => 0
        };

        var rootCommand = new RootCommand("Steam Achievement Uploader - Automate the process of defining Steam achievements")
        {
            csvFileOption,
            imagesDirectoryOption,
            sessionIdOption,
            steamLoginSecureOption,
            appIdOption,
            generateImagesOption,
            permissionOption
        };

        rootCommand.SetAction(p => ProcessAchievements(
            p.GetRequiredValue(csvFileOption),
            p.GetValue(imagesDirectoryOption),
            p.GetRequiredValue(sessionIdOption),
            p.GetRequiredValue(steamLoginSecureOption),
            p.GetRequiredValue(appIdOption),
            p.GetValue(generateImagesOption),
            p.GetValue(permissionOption))
        );

        var parseResult = rootCommand.Parse(args);
        foreach (var error in parseResult.Errors)
        {
            await Console.Error.WriteLineAsync(error.Message);
        }

        return await parseResult.InvokeAsync();
    }

    private static async Task ProcessAchievements(string csv, string? images, string sessionId, string steamLoginSecure, string appId, bool generateImages, int permission)
    {
        try
        {
            Console.WriteLine("Steam Achievement Uploader");
            Console.WriteLine("==========================");
            Console.WriteLine();

            if (!File.Exists(csv))
            {
                Console.WriteLine($"Error: CSV file not found: \n\"{csv}\", relative to current directory: \n\"{Environment.CurrentDirectory}\"");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            if (generateImages && !Directory.Exists(images))
            {
                Console.WriteLine($"Error: Images not found: \n\"{images}\", relative to current directory \n\"{Environment.CurrentDirectory}\"");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Write("Parsing CSV file... ");
            var achievements = await CsvParser.ParseAchievementsAsync(csv);
            Console.WriteLine($"found {achievements.Count} achievements");

            // Generate greyscale images if requested
            if (generateImages)
            {
                Console.WriteLine();
                ImageProcessorService.ProcessAchievementImages(images!);
                Console.WriteLine();
            }

            var session = new SteamSession
            {
                SessionId = sessionId,
                SteamLoginSecure = steamLoginSecure,
                AppId = appId
            };

            using var steamClient = new SteamApiClient(session);
            var uploadService = new AchievementUploadService(steamClient, images, permission);

            await uploadService.ProcessAchievementsAsync(achievements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
