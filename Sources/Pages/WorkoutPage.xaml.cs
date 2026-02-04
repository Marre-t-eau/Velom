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
    private int? _lastBlockIndex = null;

    internal WorkoutPage(Workout workout) : base()
	{
		InitializeComponent();
        InitializeImports();

        WorkoutView = new WorkoutView(workout);
        // Set FTP on WorkoutView to trigger transformation from percentage to watts
        WorkoutView.FTP = UserInfo.GetUserInfo().Result.FTP;
        WorkBlocksCollectionView.ItemsSource = WorkoutView.BlocksView;
        
        // Set workout name in header
        WorkoutNameLabel.Text = $"{workout.Name}";
        
        // Initialize target displays
        UpdateTargetDisplays();
        
        // Initialize progress and time info
        UpdateProgressInfo();
        UpdateTimeRemaining();
        TotalTimeLabel.Text = $"Total: {FormatTime(WorkoutView.TotalDuration)}";
        
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
        int? currentBlockIndex = GetCurrentBlockIndex();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            TimerLabel.Text = _elapsedTime.ToString(@"hh\:mm\:ss");
            
            // Update progress and time remaining
            UpdateProgressInfo();
            UpdateTimeRemaining();
            
            if (currentWorkBlock != null && currentBlockIndex.HasValue)
            {
                currentWorkBlock.TimeDone = GetTimeDoneInActualWorkBlock();
                
                // Update block highlighting when we switch to a new block
                if (currentBlockIndex != _lastBlockIndex)
                {
                    // Remove highlight from previous block
                    if (_lastBlockIndex.HasValue && _lastBlockIndex.Value < WorkoutView.BlocksView.Count)
                    {
                        WorkoutView.BlocksView[_lastBlockIndex.Value].IsCurrent = false;
                    }
                    
                    // Highlight current block
                    if (currentBlockIndex.Value < WorkoutView.BlocksView.Count)
                    {
                        var blockToScroll = WorkoutView.BlocksView[currentBlockIndex.Value];
                        blockToScroll.IsCurrent = true;
                        
                        // Auto-scroll to current block using the object itself
                        Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                        {
                            try
                            {
                                WorkBlocksCollectionView.ScrollTo(blockToScroll, 
                                    position: ScrollToPosition.Center, 
                                    animate: true);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Scroll error: {ex.Message}");
                            }
                            return false; // Don't repeat
                        });
                    }
                    
                    _lastBlockIndex = currentBlockIndex;
                }
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

    private void UpdateProgressInfo()
    {
        int? currentIndex = GetCurrentBlockIndex();
        if (currentIndex.HasValue)
        {
            int currentBlock = currentIndex.Value + 1; // +1 pour affichage humain (1-based)
            int totalBlocks = WorkoutView.BlocksView.Count;
            BlockProgressLabel.Text = $"Block {currentBlock}/{totalBlocks}";
        }
        else
        {
            BlockProgressLabel.Text = $"Block {WorkoutView.BlocksView.Count}/{WorkoutView.BlocksView.Count}";
        }
    }

    private void UpdateTimeRemaining()
    {
        uint totalDuration = WorkoutView.TotalDuration;
        uint elapsedSeconds = (uint)_preciseElapsedSeconds;
        
        if (elapsedSeconds >= totalDuration)
        {
            TimeRemainingValueLabel.Text = "0:00";
        }
        else
        {
            uint remainingSeconds = totalDuration - elapsedSeconds;
            TimeRemainingValueLabel.Text = FormatTime(remainingSeconds);
        }
    }

    private string FormatTime(uint totalSeconds)
    {
        uint minutes = totalSeconds / 60;
        uint seconds = totalSeconds % 60;
        
        if (minutes >= 60)
        {
            uint hours = minutes / 60;
            minutes = minutes % 60;
            return $"{hours}:{minutes:D2}:{seconds:D2}";
        }
        
        return $"{minutes}:{seconds:D2}";
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
        
        // Initialize first block as current
        if (WorkoutView.BlocksView.Count > 0)
        {
            var firstBlock = WorkoutView.BlocksView[0];
            firstBlock.IsCurrent = true;
            _lastBlockIndex = 0;
            
            // Scroll to first block using the object
            Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                try
                {
                    WorkBlocksCollectionView.ScrollTo(firstBlock, 
                        position: ScrollToPosition.Start, 
                        animate: false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Scroll error: {ex.Message}");
                }
                return false;
            });
        }
        
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
            
            // Reset all blocks state
            foreach (var block in WorkoutView.BlocksView)
            {
                block.IsCurrent = false;
                block.TimeDone = 0;
            }
            _lastBlockIndex = null;
            
            // Reset progress info
            UpdateProgressInfo();
            TimeRemainingValueLabel.Text = FormatTime(WorkoutView.TotalDuration);
            
            // Scroll back to top using first block object
            if (WorkoutView.BlocksView.Count > 0)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                {
                    try
                    {
                        WorkBlocksCollectionView.ScrollTo(WorkoutView.BlocksView[0], 
                            position: ScrollToPosition.Start, 
                            animate: false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Scroll error: {ex.Message}");
                    }
                    return false;
                });
            }
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

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // If workout is running, ask for confirmation
        if (_timer.Enabled)
        {
            bool confirm = await DisplayAlert("Confirm", "A workout is in progress. Do you really want to exit?", "Yes", "No");
            if (!confirm)
                return;
            
            await StopPowerControlAsync();
            await StopSessionAsync();
        }
        
        await Navigation.PopModalAsync();
    }
}