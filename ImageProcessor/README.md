# Achievement Image Processor

This C# console application automatically generates greyscale (desaturated) versions of achievement images using Magick.NET.Core.

## Purpose

Steam achievements require both a colored "achieved" icon and a greyscale "unachieved" icon. This tool automates the creation of the greyscale versions by:

1. Scanning the `achievements` folder for JPG files with numeric names (e.g., `1.jpg`, `2.jpg`)
2. Converting each image to greyscale by removing saturation
3. Saving the greyscale version with an "_unachieved" suffix (e.g., `1_unachieved.jpg`)

## Requirements

- .NET 8.0
- Magick.NET-Q16-AnyCPU package (automatically installed)

## Usage

1. Place your achievement images in the `achievements` folder with numeric names (e.g., `1.jpg`, `2.jpg`, `123.jpg`)
2. Run the application:
   ```bash
   cd AchievementImageProcessor
   dotnet run
   ```

The application will:
- Find all numeric-named JPG files in the achievements folder
- Skip any images that already have greyscale versions
- Generate greyscale versions for new images
- Report the number of processed and skipped images

## Features

- **Smart file detection**: Only processes JPG files with numeric names
- **Duplicate prevention**: Skips images that already have greyscale versions
- **Error handling**: Continues processing other images if one fails
- **Progress reporting**: Shows detailed status for each processed image

## Example Output

```
Achievement Image Processor - Generating greyscale versions...
Found 2 achievement images to process:
  Processing 2.jpg...
    Created: 2_unachieved.jpg
  Processing 1.jpg...
    Created: 1_unachieved.jpg

Processing complete!
  Processed: 2 images
  Skipped: 0 images (already exist)
```