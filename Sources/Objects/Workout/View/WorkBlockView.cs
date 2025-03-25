
using System.ComponentModel;

namespace Velom.Sources.Objects.Workout.View;

internal class WorkBlockView : WorkBlock, INotifyPropertyChanged
{
    internal WorkBlockView(WorkBlock workBlock) : base(workBlock)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private uint _timeDone;
    public uint TimeDone
    {
        get => _timeDone;
        set
        {
            if (_timeDone != value)
            {
                _timeDone = value;
                OnPropertyChanged(nameof(TimeDone));
                OnPropertyChanged(nameof(TimeDoneString));
            }
        }
    }

    public bool AsTargetPower => TargetPowerStart.HasValue || TargetPowerEnd.HasValue;
    public bool AsNoTargetPower => !AsTargetPower;
    public bool IsRamp => AsTargetPower && (TargetPowerStart.HasValue && TargetPowerEnd.HasValue && TargetPowerStart.Value != TargetPowerEnd.Value);
    public bool IsConstant => AsTargetPower && ((TargetPowerStart.HasValue && !TargetPowerEnd.HasValue) || (!TargetPowerStart.HasValue && TargetPowerEnd.HasValue) || (TargetPowerStart.HasValue && TargetPowerEnd.HasValue && TargetPowerStart.Value == TargetPowerEnd.Value));
    public bool AsTargetedCadence => TargetCadence.HasValue;
    public bool AsNoTargetedCadence => !AsTargetedCadence;
    public ushort? TargetPower => TargetPowerStart ?? TargetPowerEnd;

    public string TargetPowerStartString => TargetPowerStart?.ToString() ?? string.Empty;
    public string TargetPowerEndString => TargetPowerEnd?.ToString() ?? string.Empty;
    public string TargetPowerString => TargetPowerStart.HasValue ? TargetPowerStartString : TargetPowerEndString;
    public string TargetCadenceString => TargetCadence?.ToString() ?? string.Empty;
    public string TimeDoneString => TimeDone.ToString();
    public string DurationString => Duration.ToString();

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
