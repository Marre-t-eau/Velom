using System.Composition;
using System.Timers;
using Velom.Sources.Objects;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Objects.Workout.View;

namespace Velom.Sources.Pages;

public partial class WorkoutPage : BaseBikeControlPage
{
    WorkoutView WorkoutView { get; }
    private ushort? _actualTargetPower = null;
    private ushort? _actualCadenceTarget = null;

    internal WorkoutPage(Workout workout) : base()
	{
		InitializeComponent();
        InitializeImports();

        workout.FTP = UserInfo.GetUserInfo().Result.FTP;
        WorkoutView = new WorkoutView(workout);
        WorkBlocksCollectionView.ItemsSource = WorkoutView.BlocksView;
        
        // Initialize target displays
        UpdateTargetDisplays();
        
        SubscribeToBluetoothEvents();
    }

    protected override void OnPowerUpdated(object? sender, ushort power)
    {
        base.OnPowerUpdated(sender, power);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PowerView.Text = $"{power} W";
        });
    }

    protected override void OnCadenceUpdated(object? sender, ushort cadence)
    {
        base.OnCadenceUpdated(sender, cadence);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CadenceView.Text = $"{cadence} rpm";
        });
    }

    protected override void OnHeartRateUpdated(object? sender, ushort heartRate)
    {
        base.OnHeartRateUpdated(sender, heartRate);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HeartRateView.Text = $"{heartRate} bpm";
        });
    }

    protected override void OnTimerTick()
    {
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
                TargetPowerView.Text = $"{_actualTargetPower?.ToString() ?? "0"} W";
                await UpdateTargetPowerAsync(newTargetedPower ?? 0);
            }
            
            if (newTargetedCadence != _actualCadenceTarget)
            {
                _actualCadenceTarget = newTargetedCadence;
                TargetCadenceView.Text = $"{_actualCadenceTarget?.ToString() ?? "0"} rpm";
            }
        });
    }

    private void UpdateTargetDisplays()
    {
        _actualTargetPower = GetActualTargetPower();
        _actualCadenceTarget = GetActualCadenceTarget();
        TargetPowerView.Text = $"{_actualTargetPower?.ToString() ?? "0"} W";
        TargetCadenceView.Text = $"{_actualCadenceTarget?.ToString() ?? "0"} rpm";
    }

    protected override ushort? GetCurrentTargetPower() => _actualTargetPower;
    protected override ushort? GetCurrentTargetCadence() => _actualCadenceTarget;
    protected override int? GetCurrentBlockIndex()
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

    private async void OnStartButtonClicked(object sender, EventArgs e)
    {
        await StartPowerControlAsync(GetActualTargetPower() ?? 0);
        StartButton.IsVisible = false;
        PauseStopButtons.IsVisible = true;
        
        await StartSessionAsync(WorkoutView.Name);
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
        bool confirm = await DisplayAlert("Confirmation", "Are you sure you want to stop this workout?", "Yes", "No");
        if (confirm)
        {
            await StopPowerControlAsync();
            await StopSessionAsync();
            
            TimerLabel.Text = "00:00:00";
            StartButton.IsVisible = true;
            PauseStopButtons.IsVisible = false;
            PauseButton.Text = "Pause";
        }
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
        
        if (WorkoutView.Blocks.Count <= indice)
            return 0;
        
        uint timeDone = (uint)Math.Ceiling(elapsedTime);
        uint duration = WorkoutView.Blocks[indice].Duration;
        
        return Math.Min(timeDone, duration);
    }
}