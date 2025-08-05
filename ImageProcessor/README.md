# Achievement Image Processor

This C# console application automatically generates greyscale (desaturated) versions of achievement images using Magick.NET.Core.

## Purpose

Steam achievements require both a coloured "achieved" icon and a greyscale "unachieved" icon. This tool automates the creation of the greyscale versions by:

1. Scanning the `data` folder for JPG files (`1.jpg`)
2. Converting each image to greyscale by removing saturation
3. Saving the greyscale version with an "_unachieved" suffix (`1_unachieved.jpg`)

## Usage

1. Place your achievement images in the `data` folder with numeric names (e.g., `1.jpg`, `abc.jpg`, `123.jpg`)
2. Run the application:
   ```bash
   cd ImageProcessor
   dotnet run
   ```
