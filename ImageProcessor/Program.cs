using ImageMagick;
using System.Text.RegularExpressions;

namespace ImageProcessor;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Achievement Image Processor - Generating greyscale versions...");

        // Get the achievements folder path - check multiple possible locations
        string[] possiblePaths = {
            "achievements",                          // When running from repository root
            Path.Combine("..", "..", "..", "..", "achievements") // When running from bin/Debug/net8.0
        };
        var achievementsPath = possiblePaths.FirstOrDefault(path => Directory.Exists(path));
        if (achievementsPath == null)
        {
            Console.WriteLine("Error: Achievements folder not found. Please ensure you're running from the repository root or ImageProcessor folder.");
            return;
        }

        Console.WriteLine($"Using achievements folder: {Path.GetFullPath(achievementsPath)}");

        // Pattern to match numeric-named JPG files (e.g., 1.jpg, 2.jpg, 123.jpg)
        var numericJpgPattern = new Regex(@"^(\d+)\.jpg$", RegexOptions.IgnoreCase);

        var jpgFiles = Directory.GetFiles(achievementsPath, "*.jpg")
            .Where(file => numericJpgPattern.IsMatch(Path.GetFileName(file)))
            .Where(file => !Path.GetFileName(file).Contains("_unachieved")) // Skip already processed files
            .ToList();

        if (jpgFiles.Count == 0)
        {
            Console.WriteLine("No numeric-named JPG files found to process.");
            return;
        }

        Console.WriteLine($"Found {jpgFiles.Count} achievement images to process:");

        int processedCount = 0;
        int skippedCount = 0;

        foreach (string inputPath in jpgFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(achievementsPath, $"{fileName}_unachieved.jpg");

            // Check if greyscale version already exists
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

        Console.WriteLine("\nProcessing complete!");
        Console.WriteLine($"  Processed: {processedCount} images");
        Console.WriteLine($"  Skipped: {skippedCount} images (already exist)");
    }
}
