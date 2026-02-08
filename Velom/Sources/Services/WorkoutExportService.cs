using System.Text;
using System.Xml;
using Velom.Sources.Objects.WorkoutHistory;

namespace Velom.Sources.Services;

/// <summary>
/// Service to export workout data to TCX (Training Center XML) format
/// TCX is a standard format supported by Garmin Connect, Strava, TrainingPeaks, etc.
/// </summary>
internal interface IWorkoutExportService
{
    Task<string> ExportToTcxAsync(WorkoutSession session, List<WorkoutRecord> records);
    Task<bool> SaveTcxFileAsync(string tcxContent, string filename);
    Task<bool> ShareTcxFileAsync(WorkoutSession session, List<WorkoutRecord> records);
}

internal class WorkoutExportService : IWorkoutExportService
{
    /// <summary>
    /// Export workout data to TCX format
    /// </summary>
    public async Task<string> ExportToTcxAsync(WorkoutSession session, List<WorkoutRecord> records)
    {
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Async = true
        };

        using (var writer = XmlWriter.Create(sb, settings))
        {
            await writer.WriteStartDocumentAsync();

            // TrainingCenterDatabase root element
            await writer.WriteStartElementAsync(null, "TrainingCenterDatabase", "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2");
            await writer.WriteAttributeStringAsync("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            await writer.WriteAttributeStringAsync("xsi", "schemaLocation", null, 
                "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd");

            // Activities
            await writer.WriteStartElementAsync(null, "Activities", null);
            await writer.WriteStartElementAsync(null, "Activity", null);
            await writer.WriteAttributeStringAsync(null, "Sport", null, "Biking");

            // Id (start time)
            await writer.WriteElementStringAsync(null, "Id", null, session.StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            // Lap
            await writer.WriteStartElementAsync(null, "Lap", null);
            await writer.WriteAttributeStringAsync(null, "StartTime", null, session.StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            // Lap summary
            await writer.WriteElementStringAsync(null, "TotalTimeSeconds", null, session.TotalDurationSeconds.ToString());
            await writer.WriteElementStringAsync(null, "DistanceMeters", null, "0"); // Distance not tracked
            await writer.WriteElementStringAsync(null, "Calories", null, ((int)session.TotalKilojoules).ToString());
            await writer.WriteElementStringAsync(null, "Intensity", null, "Active");
            await writer.WriteElementStringAsync(null, "TriggerMethod", null, "Manual");

            // Track
            await writer.WriteStartElementAsync(null, "Track", null);

            // Trackpoints (records)
            foreach (var record in records)
            {
                await writer.WriteStartElementAsync(null, "Trackpoint", null);
                
                await writer.WriteElementStringAsync(null, "Time", null, 
                    record.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

                if (record.HeartRate.HasValue)
                {
                    await writer.WriteStartElementAsync(null, "HeartRateBpm", null);
                    await writer.WriteElementStringAsync(null, "Value", null, record.HeartRate.Value.ToString());
                    await writer.WriteEndElementAsync(); // HeartRateBpm
                }

                if (record.Cadence.HasValue)
                {
                    await writer.WriteElementStringAsync(null, "Cadence", null, record.Cadence.Value.ToString());
                }

                // Extensions for power data
                if (record.Power.HasValue)
                {
                    await writer.WriteStartElementAsync(null, "Extensions", null);
                    await writer.WriteStartElementAsync(null, "TPX", "http://www.garmin.com/xmlschemas/ActivityExtension/v2");
                    await writer.WriteElementStringAsync(null, "Watts", null, record.Power.Value.ToString());
                    await writer.WriteEndElementAsync(); // TPX
                    await writer.WriteEndElementAsync(); // Extensions
                }

                await writer.WriteEndElementAsync(); // Trackpoint
            }

            await writer.WriteEndElementAsync(); // Track

            // Extensions for average power and other metrics
            await writer.WriteStartElementAsync(null, "Extensions", null);
            await writer.WriteStartElementAsync(null, "LX", "http://www.garmin.com/xmlschemas/ActivityExtension/v2");
            await writer.WriteElementStringAsync(null, "AvgWatts", null, ((int)session.AveragePower).ToString());
            await writer.WriteElementStringAsync(null, "MaxWatts", null, session.MaxPower.ToString());
            await writer.WriteEndElementAsync(); // LX
            await writer.WriteEndElementAsync(); // Extensions

            await writer.WriteEndElementAsync(); // Lap

            // Notes
            if (!string.IsNullOrWhiteSpace(session.Notes))
            {
                await writer.WriteElementStringAsync(null, "Notes", null, session.Notes);
            }

            await writer.WriteEndElementAsync(); // Activity
            await writer.WriteEndElementAsync(); // Activities

            // Author
            await writer.WriteStartElementAsync(null, "Author", null);
            await writer.WriteAttributeStringAsync("xsi", "type", null, "Application_t");
            await writer.WriteElementStringAsync(null, "Name", null, "Velom");
            await writer.WriteStartElementAsync(null, "Build", null);
            await writer.WriteStartElementAsync(null, "Version", null);
            await writer.WriteElementStringAsync(null, "VersionMajor", null, "1");
            await writer.WriteElementStringAsync(null, "VersionMinor", null, "0");
            await writer.WriteEndElementAsync(); // Version
            await writer.WriteEndElementAsync(); // Build
            await writer.WriteEndElementAsync(); // Author

            await writer.WriteEndElementAsync(); // TrainingCenterDatabase
            await writer.WriteEndDocumentAsync();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Save TCX content to a file
    /// </summary>
    public async Task<bool> SaveTcxFileAsync(string tcxContent, string filename)
    {
        try
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, filename);
            await File.WriteAllTextAsync(filePath, tcxContent);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Share TCX file using platform share functionality
    /// </summary>
    public async Task<bool> ShareTcxFileAsync(WorkoutSession session, List<WorkoutRecord> records)
    {
        try
        {
            var tcxContent = await ExportToTcxAsync(session, records);
            var filename = $"Velom_{session.WorkoutName.Replace(" ", "_")}_{session.StartTime:yyyyMMdd_HHmmss}.tcx";
            var filePath = Path.Combine(FileSystem.CacheDirectory, filename);
            
            await File.WriteAllTextAsync(filePath, tcxContent);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Share Workout",
                File = new ShareFile(filePath)
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}
