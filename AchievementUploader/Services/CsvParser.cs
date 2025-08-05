using System.Globalization;
using System.Text;
using AchievementUploader.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AchievementUploader.Services;

public class CsvParser
{
    public static async Task<List<Achievement>> ParseAchievementsAsync(string csvFilePath)
    {
        var content = await File.ReadAllTextAsync(csvFilePath, Encoding.UTF8);
        using var reader = new StringReader(content);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            BadDataFound = null, // Ignore bad data
            MissingFieldFound = null, // Ignore missing fields
        };
        
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<AchievementMap>();
        
        return csv.GetRecords<Achievement>().ToList();
    }
}

public class AchievementMap : ClassMap<Achievement>
{
    public AchievementMap()
    {
        Map(m => m.Id).Index(0);
        Map(m => m.DisplayName).Index(1);
        Map(m => m.Description).Index(2);
        Map(m => m.Hidden).Index(3)
            .Convert(args => 
            {
                var value = args.Row.GetField(3) ?? "false";
                return value.Trim().ToLowerInvariant() == "true" || value.Trim() == "1";
            });
    }
}
