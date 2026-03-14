using System.Composition;
using Velom.Sources.Objects;
using Velom.Sources.Objects.WorkoutHistory;
using Velom.Resources.Strings;

namespace Velom.Sources.Pages;

public partial class ControlPage : BaseBikeControlPage
{
    private ushort? _currentTargetPower = null;

    public ControlPage() : base()
    {
        InitializeComponent();
        InitializeImports();
        SubscribeToBluetoothEvents();
    }

    protected override void OnPowerUpdated(object? sender, ushort power)
    {
        base.OnPowerUpdated(sender, power);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PowerLabel.Text = $"{power} W";
        });
    }

    protected override void OnCadenceUpdated(object? sender, ushort cadence)
    {
        base.OnCadenceUpdated(sender, cadence);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CadenceLabel.Text = $"{cadence} rpm";
        });
    }

    protected override void OnHeartRateUpdated(object? sender, ushort heartRate)
    {
        base.OnHeartRateUpdated(sender, heartRate);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HeartRateLabel.Text = $"{heartRate} bpm";
        });
    }

    protected override void OnTimerTick()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TimerLabel.Text = _elapsedTime.ToString(@"hh\:mm\:ss");
        });
    }

    protected override ushort? GetCurrentTargetPower() => _currentTargetPower;
    protected override ushort? GetCurrentTargetCadence() => null;
    protected override int? GetCurrentBlockIndex() => null;

    // Session Control Handlers
    private async void OnStartButtonClicked(object sender, EventArgs e)
    {
        if (ushort.TryParse(TargetPowerEntry.Text, out ushort power))
        {
            _currentTargetPower = power;
            await StartPowerControlAsync(power);
        }
        else
        {
            _currentTargetPower = 100;
            await StartPowerControlAsync(100);
            TargetPowerEntry.Text = "100";
        }

        await StartSessionAsync("Free Ride");
        
        StartButton.IsVisible = false;
        PauseStopButtons.IsVisible = true;
        PowerControlSection.IsEnabled = true;
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
        bool confirm = await DisplayAlert(AppResources.Confirmation, AppResources.StopSessionConfirmation, AppResources.Yes, AppResources.No);
        if (confirm)
        {
            await StopPowerControlAsync();
            await StopSessionAsync();
            
            TimerLabel.Text = "00:00:00";
            StartButton.IsVisible = true;
            PauseStopButtons.IsVisible = false;
            PowerControlSection.IsEnabled = false;
            PauseButton.Text = "Pause";
        }
    }

    // Power Control Handlers
    private async void OnTargetPowerChanged(object sender, TextChangedEventArgs e)
    {
        if (_isControlling && ushort.TryParse(e.NewTextValue, out ushort power))
        {
            _currentTargetPower = power;
            await UpdateTargetPowerAsync(power);
        }
    }

    private void OnIncrease5Power(object sender, EventArgs e)
    {
        AdjustPower(5);
    }

    private void OnDecrease5Power(object sender, EventArgs e)
    {
        AdjustPower(-5);
    }

    private void AdjustPower(int delta)
    {
        if (ushort.TryParse(TargetPowerEntry.Text, out ushort currentPower))
        {
            int newPower = currentPower + delta;
            if (newPower < 0) newPower = 0;
            if (newPower > 1000) newPower = 1000;
            TargetPowerEntry.Text = newPower.ToString();
        }
    }

    private void OnPreset50(object sender, EventArgs e) => SetPowerPreset(50);
    private void OnPreset100(object sender, EventArgs e) => SetPowerPreset(100);
    private void OnPreset150(object sender, EventArgs e) => SetPowerPreset(150);
    private void OnPreset200(object sender, EventArgs e) => SetPowerPreset(200);
    private void OnPreset250(object sender, EventArgs e) => SetPowerPreset(250);
    private void OnPreset300(object sender, EventArgs e) => SetPowerPreset(300);

    private void SetPowerPreset(ushort power)
    {
        TargetPowerEntry.Text = power.ToString();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (_isControlling)
        {
            bool confirm = await DisplayAlert(AppResources.Confirm, 
                AppResources.SessionActiveConfirmation, 
                AppResources.Yes, AppResources.No);
            
            if (confirm)
            {
                await StopPowerControlAsync();
                await StopSessionAsync();
                await Navigation.PopModalAsync();
            }
        }
        else
        {
            await Navigation.PopModalAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        if (_isControlling && _currentSession != null)
        {
            _ = Task.Run(async () =>
            {
                await StopPowerControlAsync();
                await StopSessionAsync();
            });
        }
    }
}
