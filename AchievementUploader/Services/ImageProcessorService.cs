using ImageMagick;
using System.Text.RegularExpressions;

namespace AchievementUploader.Services;

public static class ImageProcessorService
{
    public static int ProcessAchievementImages(string imagesDirectory)
    {
        Console.WriteLine("Generating greyscale _unachieved versions of achievement images...");

        if (!Directory.Exists(imagesDirectory))
        {
            Console.WriteLine($"Error: Images directory not found: {imagesDirectory}");
            return 0;
        }

        Console.WriteLine($"Using images directory: {Path.GetFullPath(imagesDirectory)}");

        // Any JPG file except those with _unachieved suffix
        var jpgPattern = new Regex(@"^.*\.jpg$", RegexOptions.IgnoreCase);

        var jpgFiles = Directory.GetFiles(imagesDirectory, "*.jpg")
            .Where(file => jpgPattern.IsMatch(Path.GetFileName(file)))
            .Where(file => !Path.GetFileName(file).Contains("_unachieved", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (jpgFiles.Count == 0)
        {
            Console.WriteLine("No JPG files found to process.");
            return 0;
        }

        Console.WriteLine($"Found {jpgFiles.Count} achievement images to process:");

        int processedCount = 0;
        int skippedCount = 0;

        foreach (string inputPath in jpgFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(imagesDirectory, $"{fileName}_unachieved.jpg");

            if (File.Exists(outputPath))
            {
                Console.WriteLine($"  Skipping {Path.GetFileName(inputPath)} - greyscale version already exists");
                skippedCount++;
                continue;
            }

            try
            {
                Console.WriteLine($"  Processing {Path.GetFileName(inputPath)}...");
                using (var image = new MagickImage(inputPath))
                {
                    // Convert to greyscale by desaturating the image
                    image.Modulate(new Percentage(100), new Percentage(0), new Percentage(100));
                    image.Write(outputPath);
                }
                Console.WriteLine($"    Created: {Path.GetFileName(outputPath)}");
                processedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error processing {Path.GetFileName(inputPath)}: {ex.Message}");
            }
        }

        Console.WriteLine($"\nImage processing complete! Processed: {processedCount} images, Skipped: {skippedCount} images");
        return processedCount;
    }
}
