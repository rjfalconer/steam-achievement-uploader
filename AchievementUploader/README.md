# Steam Achievement Uploader

A C# command-line tool that automates the process of defining achievements for Steam applications. This tool eliminates the manual work of uploading achievements through the Steam Partner portal by bulk processing CSV data and images.

![](../doc/partner-screenshot.jpg)

## Requirements

- .NET 8.0 or later
- Valid Steam Partner account with access to the app
- Steam session cookies (sessionid, steamLoginSecure)

## Installation

1. Clone the repository
2. Navigate to the `AchievementUploader` directory
3. Build the project:
   ```bash
   dotnet build
   ```

## Usage

```bash
dotnet run -- --csv <csv-file> --images <images-directory> --session-id <session-id> --steam-login-secure <steam-login> --app-id <app-id> [--generate-images]
```

### Parameters

- `--csv`: Path to the CSV file containing achievement data
- `--images`: Path to the directory containing achievement images
- `--session-id`: Steam session ID cookie value
- `--steam-login-secure`: Steam login secure cookie value  
- `--app-id`: Your Steam App ID
- `--permission`: Who is allowed to grant the achievement (0 = Client, 1 = Server, 2 = Official game server)
- `--generate-images`: (Optional) Generate greyscale _unachieved versions of achievement images

### CSV Format

The CSV file should contain achievement data in the following format:
```csv
ID,Display Name,Description,Hidden
1,First Achievement,Complete the first task,0
2,Second Achievement,Complete the second task,true
```
(You can use either "true/false" or "0/1" to represent the Hidden status)

### Image Files

Achievement images should be placed in the images directory with the following naming convention:
- Achieved icon: `{ID}.jpg` (e.g., `1.jpg`)
- Unachieved icon: `{ID}_unachieved.jpg` (e.g., `1_unachieved.jpg`)

Steam suggests that images should be 256x256 pixels in JPEG format, but this is not enforced by this tool.

## Getting Steam Session Cookies

1. Log into your Steam Partner account at https://partner.steamgames.com
2. Navigate to your app's achievement configuration page
3. Open browser developer tools (F12)
4. Go to the Application/Storage tab and find the Cookies section
5. Copy the values for:
   - `sessionid`
   - `steamLoginSecure`

## Example

```bash
dotnet run -- \
  --csv "./achievements.csv" \
  --images "./achievement_icons" \
  --session-id "[short string]" \
  --steam-login-secure "[very long string]" \
  --app-id "1234567" \
  --generate-images
```

## Notes

- The tool adds a 1-second delay between API requests to be respectful to Steam's servers
- If an achievement already exists, it will be updated with the new data
