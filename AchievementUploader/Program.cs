using System.CommandLine;
using AchievementUploader.Models;
using AchievementUploader.Services;

namespace AchievementUploader;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var csvFileOption = new Option<string>(
            name: "--csv",
            description: "Path to the CSV file containing achievement data")
        {
            IsRequired = true
        };

        var imagesDirectoryOption = new Option<string>(
            name: "--images",
            description: "Path to the directory containing achievement images")
        {
            IsRequired = true
        };

        var sessionIdOption = new Option<string>(
            name: "--session-id",
            description: "Steam session ID")
        {
            IsRequired = true
        };

        var steamLoginSecureOption = new Option<string>(
            name: "--steam-login-secure",
            description: "Steam login secure cookie value")
        {
            IsRequired = true
        };

        var appIdOption = new Option<string>(
            name: "--app-id",
            description: "Steam App ID")
        {
            IsRequired = true
        };

        var generateImagesOption = new Option<bool>(
            name: "--generate-images",
            description: "Generate greyscale _unachieved versions of achievement images before uploading")
        {
            IsRequired = false
        };

        var permissionOption = new Option<int>(
            name: "--permission",
            description: "Permission level for achievements (0 = Client, 1 = GameServer, 2 = Official Game Server)")
        {
            IsRequired = false
        };
        permissionOption.SetDefaultValue(0);

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

        rootCommand.SetHandler(async (csvFile, imagesDirectory, sessionId, steamLoginSecure, appId, generateImages, permission) =>
        {
            await ProcessAchievements(csvFile, imagesDirectory, sessionId, steamLoginSecure, appId, generateImages, permission);
        }, csvFileOption, imagesDirectoryOption, sessionIdOption, steamLoginSecureOption, appIdOption, generateImagesOption, permissionOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ProcessAchievements(string csvFile, string imagesDirectory, string sessionId, string steamLoginSecure, string appId, bool generateImages, int permission)
    {
        try
        {
            Console.WriteLine("Steam Achievement Uploader");
            Console.WriteLine("==========================");
            Console.WriteLine();

            if (!File.Exists(csvFile))
            {
                Console.WriteLine($"Error: CSV file not found: \n\"{csvFile}\", relative to current directory: \n\"{Environment.CurrentDirectory}\"");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(imagesDirectory))
            {
                Console.WriteLine($"Error: Images not found: \n\"{imagesDirectory}\", relative to current directory \n\"{Environment.CurrentDirectory}\"");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Write("Parsing CSV file... ");
            var achievements = await CsvParser.ParseAchievementsAsync(csvFile);
            Console.WriteLine($"found {achievements.Count} achievements");

            // Generate greyscale images if requested
            if (generateImages)
            {
                Console.WriteLine();
                ImageProcessorService.ProcessAchievementImages(imagesDirectory);
                Console.WriteLine();
            }

            var session = new SteamSession
            {
                SessionId = sessionId,
                SteamLoginSecure = steamLoginSecure,
                AppId = appId
            };

            using var steamClient = new SteamApiClient(session);
            var uploadService = new AchievementUploadService(steamClient, imagesDirectory, permission);

            await uploadService.ProcessAchievementsAsync(achievements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
