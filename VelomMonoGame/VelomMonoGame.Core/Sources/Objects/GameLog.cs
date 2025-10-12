using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources.Objects;

public class GameLog
{
    public List<GameLogEntry> Entries { get; } = new List<GameLogEntry>();
}

public class GameLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = null;
}

public static class GameLogEntryType
{
    public const string Start = "Start";
    public const string Pause = "Pause";
    public const string Resume = "Resume";
    public const string Stop = "Stop";
    public const string Power = "Power";
    public const string Cadence = "Cadence";
    public const string HeartRate = "HeartRate";
}
