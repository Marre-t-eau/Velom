using System.Composition;
using System.Timers;
using Velom.Sources.Objects;
using Velom.Sources.Objects.WorkoutHistory;

namespace Velom.Sources.Pages;

public abstract class BaseBikeControlPage : ContentPage
{
    [Import]
    protected IBluetoothManager BluetoothManager { get; set; }
    
    [Import]
    protected IWorkoutHistoryService HistoryService { get; set; }
    
    protected ushort? _currentPower = null;
    protected ushort? _currentCadence = null;
    protected ushort? _currentHeartRate = null;

    protected bool _isControlling = false;

    // Session tracking
    protected System.Timers.Timer _timer;
    protected System.Timers.Timer _recordingTimer;
    protected TimeSpan _elapsedTime;
    protected double _preciseElapsedSeconds = 0.0;
    internal WorkoutSession? _currentSession = null;
    
    // For smart recording - track last recorded values
    private ushort? _lastRecordedPower = null;
    private ushort? _lastRecordedCadence = null;
    private ushort? _lastRecordedHeartRate = null;
    protected const int RECORDING_INTERVAL_MS = 200; // 5 Hz (5 recordings per second)

    protected BaseBikeControlPage()
    {
        _timer = new System.Timers.Timer(1000); // 1 second interval for UI updates
        _timer.Elapsed += (sender, e) => 
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            OnTimerTick();
        };
        
        _recordingTimer = new System.Timers.Timer(RECORDING_INTERVAL_MS);
        _recordingTimer.Elapsed += (sender, e) =>
        {
            _preciseElapsedSeconds += RECORDING_INTERVAL_MS / 1000.0;
            
            if (_currentSession != null)
            {
                _ = RecordDataPointSmart();
            }
        };
    }

    protected void InitializeImports()
    {
        App.Container.SatisfyImports(this);
    }

    protected virtual void SubscribeToBluetoothEvents()
    {
        BluetoothManager.PowerUpdated += OnPowerUpdated;
        BluetoothManager.CadenceUpdated += OnCadenceUpdated;
        BluetoothManager.HeartRateUpdated += OnHeartRateUpdated;
    }

    protected virtual void UnsubscribeFromBluetoothEvents()
    {
        BluetoothManager.PowerUpdated -= OnPowerUpdated;
        BluetoothManager.CadenceUpdated -= OnCadenceUpdated;
        BluetoothManager.HeartRateUpdated -= OnHeartRateUpdated;
    }

    protected virtual void OnPowerUpdated(object? sender, ushort power)
    {
        _currentPower = power;
    }

    protected virtual void OnCadenceUpdated(object? sender, ushort cadence)
    {
        _currentCadence = cadence;
    }

    protected virtual void OnHeartRateUpdated(object? sender, ushort heartRate)
    {
        _currentHeartRate = heartRate;
    }

    protected async Task StartPowerControlAsync(ushort? targetPower = null)
    {
        if (!_isControlling)
        {
            await BluetoothManager.StartControllingPower();
            _isControlling = true;
        }
        
        if (targetPower.HasValue)
        {
            await BluetoothManager.SetPower(targetPower.Value);
        }
    }

    protected async Task StopPowerControlAsync()
    {
        if (_isControlling)
        {
            await BluetoothManager.StopControllingPower();
            _isControlling = false;
        }
    }

    protected async Task UpdateTargetPowerAsync(ushort targetPower)
    {
        await BluetoothManager.SetPower(targetPower);
    }

    // Session management
    protected async Task StartSessionAsync(string workoutName)
    {
        var userInfo = await UserInfo.GetUserInfo();
        _currentSession = await HistoryService.CreateSessionAsync(workoutName, userInfo.FTP);
        
        _elapsedTime = TimeSpan.Zero;
        _preciseElapsedSeconds = 0.0;
        
        _timer.Start();
        _recordingTimer.Start();
    }

    protected async Task StopSessionAsync()
    {
        _timer.Stop();
        _recordingTimer.Stop();
        
        if (_currentSession != null)
        {
            await FinalizeWorkoutSession();
        }
        
        _elapsedTime = TimeSpan.Zero;
        _preciseElapsedSeconds = 0.0;
    }

    // Abstract/virtual methods for derived classes to implement
    protected virtual void OnTimerTick()
    {
        // Default: do nothing, derived classes can override
    }

    protected abstract ushort? GetCurrentTargetPower();
    protected abstract ushort? GetCurrentTargetCadence();
    protected abstract int? GetCurrentBlockIndex();

    // Smart recording
    private async Task RecordDataPointSmart()
    {
        if (_currentSession == null)
            return;

        bool shouldRecord = false;
        
        double fractionalPart = _preciseElapsedSeconds - Math.Floor(_preciseElapsedSeconds);
        bool isFullSecond = fractionalPart < 0.1;
        
        if (isFullSecond)
        {
            shouldRecord = true;
        }
        
        if (_currentPower.HasValue && _lastRecordedPower.HasValue)
        {
            if (_currentPower.Value != _lastRecordedPower.Value)
            {
                shouldRecord = true;
            }
        }
        else if (_currentPower.HasValue != _lastRecordedPower.HasValue)
        {
            shouldRecord = true;
        }

        if (!shouldRecord)
            return;

        var record = new WorkoutRecord
        {
            WorkoutSessionId = _currentSession.Id,
            TimestampSeconds = _preciseElapsedSeconds,
            Timestamp = DateTime.Now,
            Power = _currentPower,
            TargetPower = GetCurrentTargetPower(),
            Cadence = _currentCadence,
            TargetCadence = GetCurrentTargetCadence(),
            HeartRate = _currentHeartRate,
            CurrentBlockIndex = GetCurrentBlockIndex()
        };

        await HistoryService.AddRecordAsync(record);
        
        _lastRecordedPower = _currentPower;
        _lastRecordedCadence = _currentCadence;
        _lastRecordedHeartRate = _currentHeartRate;
    }

    private async Task FinalizeWorkoutSession()
    {
        if (_currentSession == null)
            return;

        _currentSession.EndTime = DateTime.Now;
        _currentSession.TotalDurationSeconds = (int)_preciseElapsedSeconds;
        _currentSession.IsCompleted = true;

        var stats = await HistoryService.CalculateStatisticsAsync(_currentSession.Id);
        
        _currentSession.AveragePower = stats.AveragePower;
        _currentSession.MaxPower = stats.MaxPower;
        _currentSession.AverageCadence = stats.AverageCadence;
        _currentSession.MaxCadence = stats.MaxCadence;
        _currentSession.AverageHeartRate = stats.AverageHeartRate;
        _currentSession.MaxHeartRate = stats.MaxHeartRate;
        _currentSession.TotalKilojoules = stats.TotalKilojoules;
        _currentSession.NormalizedPower = stats.NormalizedPower;
        
        _currentSession.IntensityFactor = WorkoutHistoryService.CalculateIntensityFactor(
            stats.NormalizedPower, 
            _currentSession.FTP);
        _currentSession.TSS = WorkoutHistoryService.CalculateTSS(
            stats.NormalizedPower, 
            _currentSession.FTP, 
            _currentSession.TotalDurationSeconds);

        await HistoryService.UpdateSessionAsync(_currentSession);
        
        _currentSession = null;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnsubscribeFromBluetoothEvents();
        
        _timer?.Stop();
        _recordingTimer?.Stop();
    }
}
