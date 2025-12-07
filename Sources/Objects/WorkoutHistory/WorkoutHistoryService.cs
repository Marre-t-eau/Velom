using SQLite;
using System.Composition;

namespace Velom.Sources.Objects.WorkoutHistory;

/// <summary>
/// Service for managing workout history database operations
/// </summary>
internal interface IWorkoutHistoryService
{
    Task InitializeAsync();
    Task<WorkoutSession> CreateSessionAsync(string workoutName, ushort ftp);
    Task UpdateSessionAsync(WorkoutSession session);
    Task AddRecordAsync(WorkoutRecord record);
    Task<List<WorkoutSession>> GetAllSessionsAsync();
    Task<WorkoutSession?> GetSessionAsync(int sessionId);
    Task<List<WorkoutRecord>> GetSessionRecordsAsync(int sessionId);
    Task DeleteSessionAsync(int sessionId);
    Task<WorkoutSessionStatistics> CalculateStatisticsAsync(int sessionId);
}

/// <summary>
/// Statistics calculated from workout records
/// </summary>
internal class WorkoutSessionStatistics
{
    public double AveragePower { get; set; }
    public ushort MaxPower { get; set; }
    public double AverageCadence { get; set; }
    public ushort MaxCadence { get; set; }
    public double AverageHeartRate { get; set; }
    public ushort MaxHeartRate { get; set; }
    public double TotalKilojoules { get; set; }
    public double NormalizedPower { get; set; }
    public double IntensityFactor { get; set; }
    public double TSS { get; set; }
}

[Export(typeof(IWorkoutHistoryService))]
[Shared]
internal class WorkoutHistoryService : IWorkoutHistoryService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public WorkoutHistoryService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "workouthistory.db3");
    }

    public async Task InitializeAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<WorkoutSession>();
        await _database.CreateTableAsync<WorkoutRecord>();
    }

    public async Task<WorkoutSession> CreateSessionAsync(string workoutName, ushort ftp)
    {
        await InitializeAsync();

        var session = new WorkoutSession
        {
            WorkoutName = workoutName,
            StartTime = DateTime.Now,
            FTP = ftp,
            IsCompleted = false
        };

        await _database!.InsertAsync(session);
        return session;
    }

    public async Task UpdateSessionAsync(WorkoutSession session)
    {
        await InitializeAsync();
        await _database!.UpdateAsync(session);
    }

    public async Task AddRecordAsync(WorkoutRecord record)
    {
        await InitializeAsync();
        await _database!.InsertAsync(record);
    }

    public async Task<List<WorkoutSession>> GetAllSessionsAsync()
    {
        await InitializeAsync();
        return await _database!.Table<WorkoutSession>()
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<WorkoutSession?> GetSessionAsync(int sessionId)
    {
        await InitializeAsync();
        return await _database!.Table<WorkoutSession>()
            .Where(s => s.Id == sessionId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkoutRecord>> GetSessionRecordsAsync(int sessionId)
    {
        await InitializeAsync();
        return await _database!.Table<WorkoutRecord>()
            .Where(r => r.WorkoutSessionId == sessionId)
            .OrderBy(r => r.TimestampSeconds)
            .ToListAsync();
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        await InitializeAsync();
        
        // Delete all records for this session
        await _database!.ExecuteAsync(
            "DELETE FROM workout_records WHERE WorkoutSessionId = ?", 
            sessionId);
        
        // Delete the session
        await _database!.DeleteAsync<WorkoutSession>(sessionId);
    }

    public async Task<WorkoutSessionStatistics> CalculateStatisticsAsync(int sessionId)
    {
        await InitializeAsync();
        var records = await GetSessionRecordsAsync(sessionId);

        var stats = new WorkoutSessionStatistics();

        if (records.Count == 0)
            return stats;

        // Calculate basic statistics
        var powerRecords = records.Where(r => r.Power.HasValue).Select(r => r.Power!.Value).ToList();
        var cadenceRecords = records.Where(r => r.Cadence.HasValue).Select(r => r.Cadence!.Value).ToList();
        var hrRecords = records.Where(r => r.HeartRate.HasValue).Select(r => r.HeartRate!.Value).ToList();

        if (powerRecords.Any())
        {
            stats.AveragePower = powerRecords.Select(p => (double)p).Average();
            stats.MaxPower = powerRecords.Max();
            
            // Calculate total energy (kilojoules)
            // 1 watt-second = 1 joule, so average watts * seconds / 1000 = kJ
            stats.TotalKilojoules = stats.AveragePower * records.Count / 1000.0;

            // Calculate Normalized Power (NP)
            // NP is calculated as the 4th root of the average of the 4th power of power values
            // This is a simplified version - proper NP uses 30-second rolling average
            stats.NormalizedPower = CalculateNormalizedPower(powerRecords);
        }

        if (cadenceRecords.Any())
        {
            stats.AverageCadence = cadenceRecords.Select(c => (double)c).Average();
            stats.MaxCadence = cadenceRecords.Max();
        }

        if (hrRecords.Any())
        {
            stats.AverageHeartRate = hrRecords.Select(h => (double)h).Average();
            stats.MaxHeartRate = hrRecords.Max();
        }

        return stats;
    }

    /// <summary>
    /// Calculate Normalized Power according to TrainingPeaks methodology
    /// </summary>
    private double CalculateNormalizedPower(List<ushort> powerValues)
    {
        if (powerValues.Count == 0)
            return 0;

        // For simplicity, we'll use a 30-second rolling average
        // In a production app, you'd want to implement proper 30-second windows
        const int windowSize = 30;
        var rollingAverages = new List<double>();

        for (int i = 0; i <= powerValues.Count - windowSize; i++)
        {
            var window = powerValues.Skip(i).Take(windowSize);
            rollingAverages.Add(window.Select(w => (double)w).Average());
        }

        if (rollingAverages.Count == 0)
            return powerValues.Select(p => (double)p).Average();

        // Calculate the 4th power of each value, then average, then take 4th root
        var fourthPowerAverage = rollingAverages.Average(v => Math.Pow(v, 4));
        return Math.Pow(fourthPowerAverage, 0.25);
    }

    /// <summary>
    /// Calculate Training Stress Score (TSS)
    /// TSS = (seconds × NP × IF) / (FTP × 3600) × 100
    /// where IF (Intensity Factor) = NP / FTP
    /// </summary>
    public static double CalculateTSS(double normalizedPower, ushort ftp, int durationSeconds)
    {
        if (ftp == 0)
            return 0;

        double intensityFactor = normalizedPower / ftp;
        double tss = (durationSeconds * normalizedPower * intensityFactor) / (ftp * 3600.0) * 100.0;
        return Math.Round(tss, 1);
    }

    /// <summary>
    /// Calculate Intensity Factor (IF)
    /// IF = Normalized Power / FTP
    /// </summary>
    public static double CalculateIntensityFactor(double normalizedPower, ushort ftp)
    {
        if (ftp == 0)
            return 0;

        return Math.Round(normalizedPower / ftp, 2);
    }
}
