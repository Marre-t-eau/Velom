using SQLite;

namespace Velom.Sources.Objects.WorkoutHistory;

/// <summary>
/// Represents a single data point recorded during a workout
/// Follows the structure similar to FIT file records
/// </summary>
[Table("workout_records")]
internal class WorkoutRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to WorkoutSession
    /// </summary>
    [Indexed]
    public int WorkoutSessionId { get; set; }

    /// <summary>
    /// Timestamp of this record (relative to workout start in seconds)
    /// Now supports sub-second precision for high-frequency recording (5 Hz)
    /// </summary>
    public double TimestampSeconds { get; set; }

    /// <summary>
    /// Actual timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Power in watts at this moment
    /// </summary>
    public ushort? Power { get; set; }

    /// <summary>
    /// Target power in watts at this moment
    /// </summary>
    public ushort? TargetPower { get; set; }

    /// <summary>
    /// Cadence in RPM at this moment
    /// </summary>
    public ushort? Cadence { get; set; }

    /// <summary>
    /// Target cadence in RPM at this moment
    /// </summary>
    public ushort? TargetCadence { get; set; }

    /// <summary>
    /// Heart rate in BPM at this moment
    /// </summary>
    public ushort? HeartRate { get; set; }

    /// <summary>
    /// Speed in km/h (if available)
    /// </summary>
    public double? Speed { get; set; }

    /// <summary>
    /// Distance in meters (if available)
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Current workout block index
    /// </summary>
    public int? CurrentBlockIndex { get; set; }
}
