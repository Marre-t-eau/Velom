using System.Composition;
using System.Timers;
using Velom.Sources.Objects;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Objects.Workout.View;

namespace Velom.Sources.Pages;

public partial class WorkoutPage : ContentPage
{
    [Import]
    private IBluetoothManager BluetoothManager { get; init; }

    WorkoutView WorkoutView { get; }
    private System.Timers.Timer _timer;
    private TimeSpan _elapsedTime;

    private ushort? _actualTargetPower = null;
    private ushort? _actualCadenceTarget = null;

    internal WorkoutPage(Workout workout)
	{
		InitializeComponent();
        App.Container.SatisfyImports(this);
        _timer = new System.Timers.Timer(1000); // 1 second interval
        _timer.Elapsed += OnTimerElapsed;
        workout.FTP = UserInfo.GetUserInfo().Result.FTP;
        WorkoutView = new WorkoutView(workout);
        WorkBlocksCollectionView.ItemsSource = WorkoutView.BlocksView;
        TargetPowerView.Text = string.Format("Target power : {0}", GetActualTargetPower()?.ToString() ?? "0");
        TargetCadenceView.Text = string.Format("Target cadence : {0}", GetActualCadenceTarget()?.ToString() ?? "0");
        BluetoothManager.PowerUpdated += (sender, power) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerView.Text = "Power: " + power;
            });
        };
        BluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CadenceView.Text = "Cadence: " + cadence;
            });
        };
        BluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
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
        _timer.Start();
    }

    private void OnPauseButtonClicked(object sender, EventArgs e)
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            PauseButton.Text = "Resume";
        }
        else
        {
            _timer.Start();
            PauseButton.Text = "Pause";
        }
    }

    private async void OnStopButtonClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmation", "Are you sure you want to stop?", "Yes", "No");
        if (confirm)
        {
            _timer.Stop();
            _elapsedTime = TimeSpan.Zero;
            TimerLabel.Text = "00:00:00";
            StartButton.IsVisible = true;
            PauseStopButtons.IsVisible = false;
            await BluetoothManager.StopControllingPower();
        }
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
        ushort? newTargetedPower = GetActualTargetPower();
        ushort? newTargetedCadence = GetActualCadenceTarget();
        WorkBlockView? currentWorkBlock = GetCurrentWorkBlock();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            TimerLabel.Text = _elapsedTime.ToString();
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

    private ushort? GetActualTargetPower()
    {
        uint elapsedTime = (uint)_elapsedTime.TotalSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count < indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }

        if (WorkoutView.Blocks.Count < indice)
            return null;

        if (WorkoutView.BlocksView[indice].IsConstant)
            return WorkoutView.BlocksView[indice].TargetPower;

        long result = WorkoutView.BlocksView[indice].TargetPowerStart + (elapsedTime * (WorkoutView.BlocksView[indice].TargetPowerEnd - WorkoutView.BlocksView[indice].TargetPowerStart) / WorkoutView.BlocksView[indice].Duration) ?? 0;

        return Workout.ToUpper5((ushort)result);
    }

    private ushort? GetActualCadenceTarget()
    {
        uint elapsedTime = (uint)_elapsedTime.TotalSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count < indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        if (WorkoutView.Blocks.Count < indice)
            return null;

       return WorkoutView.BlocksView[indice].TargetCadence;
    }

    private WorkBlockView? GetCurrentWorkBlock()
    {
        uint elapsedTime = (uint)_elapsedTime.TotalSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count < indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        if (WorkoutView.Blocks.Count < indice)
            return null;

        return WorkoutView.BlocksView[indice];
    }

    private uint GetTimeDoneInActualWorkBlock()
    {
        uint elapsedTime = (uint)_elapsedTime.TotalSeconds;
        int indice = 0;
        while (WorkoutView.Blocks.Count < indice && elapsedTime > WorkoutView.Blocks[indice].Duration)
        {
            elapsedTime -= WorkoutView.Blocks[indice].Duration;
            indice++;
        }
        return elapsedTime;
    }
}