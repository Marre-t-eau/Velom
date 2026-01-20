using System.ComponentModel;
using Microsoft.Maui.Graphics;

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
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }

    private bool _isCurrent;
    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent != value)
            {
                _isCurrent = value;
                OnPropertyChanged(nameof(IsCurrent));
                OnPropertyChanged(nameof(BlockOpacity));
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(BackgroundColor));
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

    public bool IsCompleted => TimeDone >= Duration;
    public double BlockOpacity => IsCompleted ? 0.5 : 1.0;
    public Color BorderColor => IsCurrent ? Color.FromArgb("#2D8B8E") : (IsCompleted ? Color.FromArgb("#E0E0E0") : Color.FromArgb("#BDBDBD"));
    public Color BackgroundColor => IsCurrent ? Color.FromArgb("#E0F2F2") : Colors.White;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
