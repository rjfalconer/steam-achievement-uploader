# Steam Achievement Uploader

A C# solution containing tools for Steam app developers to automate achievement configuration.

![](doc/partner-screenshot.jpg)

## Projects

This repository contains two related tools:

### [AchievementUploader](AchievementUploader/README.md)
The main command-line tool that automates the process of defining achievements for Steam applications by:
- Reading achievement data from CSV files
- Uploading achievement icons (achieved and unachieved states)
- Creating or updating achievements via Steam's partner API
- Optionally generating greyscale _unachieved images automatically

### [ImageProcessor](ImageProcessor/README.md)
A standalone utility that generates greyscale (desaturated) versions of achievement images for the unachieved state. This functionality is also available as an option in the main AchievementUploader tool.

## Quick Start

For most users, the `AchievementUploader` tool with the `--generate-images` option provides a complete solution:

1. Navigate to the `AchievementUploader` directory
2. Build the project: `dotnet build`
3. Run with your data: 
   ```bash
   dotnet run -- --csv achievements.csv --images ./achievements --session-id <your-session> --steam-login-secure <your-login> --app-id <your-app-id> --generate-images
   ```

See the [AchievementUploader README](AchievementUploader/README.md) for detailed usage instructions.

## Sample data

The repository includes sample data to demonstrate the expected format:
- `data/achievements.csv` - Example achievement definitions
- `data/` - Sample achievement icons

## Requirements

- .NET 8.0 or later
- Valid Steam Partner account with app access
- Steam session cookies from partner.steamgames.com
