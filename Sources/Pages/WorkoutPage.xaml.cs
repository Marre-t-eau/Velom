using System.Composition;
using System.Timers;
using Velom.Sources.Objects;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Objects.Workout.View;
using Velom.Sources.Objects.WorkoutHistory;

namespace Velom.Sources.Pages;

public partial class WorkoutPage : ContentPage
{
    [Import]
    private IBluetoothManager BluetoothManager { get; init; }

    [Import]
    private IWorkoutHistoryService HistoryService { get; init; }

    WorkoutView WorkoutView { get; }
    private System.Timers.Timer _timer;
    private System.Timers.Timer _recordingTimer;
    private TimeSpan _elapsedTime;
    private double _preciseElapsedSeconds = 0.0;

    private ushort? _actualTargetPower = null;
    private ushort? _actualCadenceTarget = null;

    private WorkoutSession? _currentSession = null;
    private ushort? _currentPower = null;
    private ushort? _currentCadence = null;
    private ushort? _currentHeartRate = null;

    // For smart recording - track last recorded values
    private ushort? _lastRecordedPower = null;
    private ushort? _lastRecordedCadence = null;
    private ushort? _lastRecordedHeartRate = null;
    private const int RECORDING_INTERVAL_MS = 200; // 5 Hz (5 recordings per second)

    internal WorkoutPage(Workout workout)
	{
		InitializeComponent();
        App.Container.SatisfyImports(this);
        _timer = new System.Timers.Timer(1000); // 1 second interval for UI updates
        _timer.Elapsed += OnTimerElapsed;
        
        // Separate timer for high-frequency data recording (5 Hz)
        _recordingTimer = new System.Timers.Timer(RECORDING_INTERVAL_MS);
        _recordingTimer.Elapsed += OnRecordingTimerElapsed;
        
        workout.FTP = UserInfo.GetUserInfo().Result.FTP;
        WorkoutView = new WorkoutView(workout);
        WorkBlocksCollectionView.ItemsSource = WorkoutView.BlocksView;
        TargetPowerView.Text = string.Format("Target power : {0}", GetActualTargetPower()?.ToString() ?? "0");
        TargetCadenceView.Text = string.Format("Target cadence : {0}", GetActualCadenceTarget()?.ToString() ?? "0");
        BluetoothManager.PowerUpdated += (sender, power) =>
        {
            _currentPower = power;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerView.Text = "Power: " + power;
            });
        };
        BluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            _currentCadence = cadence;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CadenceView.Text = "Cadence: " + cadence;
            });
        };
        BluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
            _currentHeartRate = heartRate;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HeartRateView.Text = "Heart rate: " + heartRate;
            });
        };
    }

    private async void OnStartButtonClicked(object sender, EventArgs e)
    {
        await BluetoothManager.StartControllingPower();
        await BluetoothManager.SetPower(GetActualTargetPower() ?? 0);
        StartButton.IsVisible = false;
        PauseStopButtons.IsVisible = true;
        
        // Create workout session
        var userInfo = await UserInfo.GetUserInfo();
        _currentSession = await HistoryService.CreateSessionAsync(
            WorkoutView.Name, 
            userInfo.FTP);
        
        _timer.Start();
        _recordingTimer.Start(); // Start high-frequency recording
    }

    private void OnPauseButtonClicked(object sender, EventArgs e)
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _recordingTimer.Stop();
            PauseButton.Text = "Resume";
        }
        else
        {
            _timer.Start();
            _recordingTimer.Start();
            PauseButton.Text = "Pause";
        }
    }

    private async void OnStopButtonClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmation", "Are you sure you want to stop?", "Yes", "No");
        if (confirm)
        {
            _timer.Stop();
            _recordingTimer.Stop();
            
            // Save final workout session data
            if (_currentSession != null)
            {
                await FinalizeWorkoutSession(false);
            }
            
            _elapsedTime = TimeSpan.Zero;
            _preciseElapsedSeconds = 0.0;
            TimerLabel.Text = "00:00:00";
            StartButton.IsVisible = true;
            PauseStopButtons.IsVisible = false;
            await BluetoothManager.StopControllingPower();
        }
    }

    /// <summary>
    /// High-frequency timer for data recording (5 Hz)
    /// Uses smart recording to avoid unnecessary database writes
    /// </summary>
    private async void OnRecordingTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _preciseElapsedSeconds += RECORDING_INTERVAL_MS / 1000.0;
        
        // Record data point with smart recording logic
        if (_currentSession != null)
        {
            await RecordDataPointSmart();
        }
    }

    /// <summary>
    /// UI update timer (1 Hz - every second)
    /// </summary>
    private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
        ushort? newTargetedPower = GetActualTargetPower();
        ushort? newTargetedCadence = GetActualCadenceTarget();
        WorkBlockView? currentWorkBlock = GetCurrentWorkBlock();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            TimerLabel.Text = _elapsedTime.ToString(@"hh\:mm\:ss");
            if (currentWorkBlock != null)
            {
                currentWorkBlock.TimeDone = GetTimeDoneInActualWorkBlock();
            }
            if (newTargetedPower != _actualTargetPower)
            {
                _actualTargetPower = newTargetedPower;
                TargetPowerView.Text = string.Format("Target power : {0}", _actualTargetPower?.ToString() ?? "0");
                await BluetoothManager.SetPower(newTargetedPower ?? 0);
            }
            if (newTargetedCadence != _actualCadenceTarget)
            {
                _actualCadenceTarget = newTargetedCadence;
                TargetCadenceView.Text = string.Format("Target cadence : {0}", _actualCadenceTarget?.ToString() ?? "0");
            }
        });
    }

    /// <summary>
    /// Smart recording:
    /// - Always record every second (for consistent time series)
    /// - Record more frequently when power changes
    /// This captures peaks and variations while reducing unnecessary writes
    /// </summary>
    private async Task RecordDataPointSmart()
    {
        if (_currentSession == null)
            return;

        bool shouldRecord = false;
        
        // Calculate if we're at a full second mark (with small tolerance for floating point)
        double fractionalPart = _preciseElapsedSeconds - Math.Floor(_preciseElapsedSeconds);
        bool isFullSecond = fractionalPart < 0.1; // Within 100ms of a full second
        
        if (isFullSecond)
        {
            shouldRecord = true;
        }
        
        // Smart recording: also record if power changed significantly
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
            TargetPower = _actualTargetPower,
            Cadence = _currentCadence,
            TargetCadence = _actualCadenceTarget,
            HeartRate = _currentHeartRate,
            CurrentBlockIndex = GetCurrentBlockIndex()
        };

        await HistoryService.AddRecordAsync(record);
        
        // Update last recorded values
        _lastRecordedPower = _currentPower;
        _lastRecordedCadence = _currentCadence;
        _lastRecordedHeartRate = _currentHeartRate;
    }

    private async Task FinalizeWorkoutSession(bool isCompleted)
    {
        if (_currentSession == null)
            return;

        _currentSession.EndTime = DateTime.Now;
        _currentSession.TotalDurationSeconds = (int)_preciseElapsedSeconds;
        _currentSession.IsCompleted = isCompleted;

        // Calculate statistics
        var stats = await HistoryService.CalculateStatisticsAsync(_currentSession.Id);
        
        _currentSession.AveragePower = stats.AveragePower;
        _currentSession.MaxPower = stats.MaxPower;
        _currentSession.AverageCadence = stats.AverageCadence;
        _currentSession.MaxCadence = stats.MaxCadence;
        _currentSession.AverageHeartRate = stats.AverageHeartRate;
        _currentSession.MaxHeartRate = stats.MaxHeartRate;
        _currentSession.TotalKilojoules = stats.TotalKilojoules;
        _currentSession.NormalizedPower = stats.NormalizedPower;
        
        // Calculate IF and TSS
        _currentSession.IntensityFactor = WorkoutHistoryService.CalculateIntensityFactor(
            stats.NormalizedPower, 
            _currentSession.FTP);
        _currentSession.TSS = WorkoutHistoryService.CalculateTSS(
            stats.NormalizedPower, 
            _currentSession.FTP, 
            _currentSession.TotalDurationSeconds);

        await HistoryService.UpdateSessionAsync(_currentSession);
    }

    private int? GetCurrentBlockIndex()
    {
        double elapsedTime = _preciseElapsedSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count > indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        
        if (WorkoutView.Blocks.Count <= indice)
            return null;
            
        return indice;
    }

    private ushort? GetActualTargetPower()
    {
        double elapsedTime = _preciseElapsedSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count > indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }

        if (WorkoutView.Blocks.Count <= indice)
            return null;

        if (WorkoutView.BlocksView[indice].IsConstant)
            return WorkoutView.BlocksView[indice].TargetPower;

        var startPower = WorkoutView.BlocksView[indice].TargetPowerStart ?? 0;
        var endPower = WorkoutView.BlocksView[indice].TargetPowerEnd ?? 0;
        var duration = WorkoutView.BlocksView[indice].Duration;

        if (duration == 0)
            return (ushort)startPower;

        long powerDiff = endPower - startPower;
        long result = startPower + (long)(elapsedTime * powerDiff / duration);

        return Workout.ToUpper5((ushort)result);
    }

    private ushort? GetActualCadenceTarget()
    {
        double elapsedTime = _preciseElapsedSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count > indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        if (WorkoutView.Blocks.Count <= indice)
            return null;

       return WorkoutView.BlocksView[indice].TargetCadence;
    }

    private WorkBlockView? GetCurrentWorkBlock()
    {
        double elapsedTime = _preciseElapsedSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count > indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        if (WorkoutView.Blocks.Count <= indice)
            return null;

        return WorkoutView.BlocksView[indice];
    }

    private uint GetTimeDoneInActualWorkBlock()
    {
        double elapsedTime = _preciseElapsedSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count > indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        return (uint)elapsedTime;
    }
}