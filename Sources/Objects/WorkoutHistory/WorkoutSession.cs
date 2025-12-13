using SQLite;

namespace Velom.Sources.Objects.WorkoutHistory;

/// <summary>
/// Represents a completed workout session with all recorded data
/// </summary>
[Table("workout_sessions")]
public class WorkoutSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Name of the workout
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Start time of the workout
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the workout
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Total duration in seconds
    /// </summary>
    public int TotalDurationSeconds { get; set; }

    /// <summary>
    /// User's FTP at the time of the workout
    /// </summary>
    public ushort FTP { get; set; }

    /// <summary>
    /// Average power in watts
    /// </summary>
    public double AveragePower { get; set; }

    /// <summary>
    /// Maximum power in watts
    /// </summary>
    public ushort MaxPower { get; set; }

    /// <summary>
    /// Average cadence in RPM
    /// </summary>
    public double AverageCadence { get; set; }

    /// <summary>
    /// Maximum cadence in RPM
    /// </summary>
    public ushort MaxCadence { get; set; }

    /// <summary>
    /// Average heart rate in BPM
    /// </summary>
    public double AverageHeartRate { get; set; }

    /// <summary>
    /// Maximum heart rate in BPM
    /// </summary>
    public ushort MaxHeartRate { get; set; }

    /// <summary>
    /// Total energy expended in kilojoules
    /// </summary>
    public double TotalKilojoules { get; set; }

    /// <summary>
    /// Normalized Power (NP) - weighted average power
    /// </summary>
    public double NormalizedPower { get; set; }

    /// <summary>
    /// Training Stress Score
    /// </summary>
    public double TSS { get; set; }

    /// <summary>
    /// Intensity Factor
    /// </summary>
    public double IntensityFactor { get; set; }

    /// <summary>
    /// Whether the workout was completed or stopped early
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Additional notes about the workout
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
