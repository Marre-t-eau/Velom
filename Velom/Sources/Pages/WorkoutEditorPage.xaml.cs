using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Velom.Sources.Messages;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Services;
using Velom.Resources.Strings;

namespace Velom.Sources.Pages;

public partial class WorkoutEditorPage : ContentPage
{
    private Workout _workout;
    private ObservableCollection<WorkBlock> _blocks;
    private bool _isWattsMode = false; // true = Watts, false = % FTP

    internal WorkoutEditorPage(Workout workout)
    {
        InitializeComponent();
        
        _workout = new Workout(workout); // Create a copy to edit
        
        WorkoutNameEntry.Text = _workout.Name;
        
        _blocks = new ObservableCollection<WorkBlock>(_workout.Blocks);
        
        // Determine initial power type from first block
        if (_blocks.Count > 0 && _blocks[0].PowerType.HasValue)
        {
            _isWattsMode = _blocks[0].PowerType.Value == WorkBlock.TargetPowerType.Watts;
            PowerTypeSwitch.IsToggled = _isWattsMode;
        }

        BlocksCollectionView.ItemsSource = _blocks;
        UpdateSummary();
    }

    private void OnPowerTypeSwitchToggled(object sender, ToggledEventArgs e)
    {
        _isWattsMode = e.Value;
        
        // Update power type for all existing blocks
        foreach (var block in _blocks)
        {
            block.PowerType = _isWattsMode ? WorkBlock.TargetPowerType.Watts : WorkBlock.TargetPowerType.PercentFTP;
        }
    }

    private void OnAddBlockClicked(object sender, EventArgs e)
    {
        var newBlock = new WorkBlock
        {
            Duration = 60,
            TargetPowerStart = 100, // 100W or 100% FTP
            TargetPowerEnd = 100,
            TargetCadence = 90,
            PowerType = _isWattsMode ? WorkBlock.TargetPowerType.Watts : WorkBlock.TargetPowerType.PercentFTP
        };
        
        _blocks.Add(newBlock);
        UpdateSummary();
    }

    private async void OnDeleteBlockClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WorkBlock block)
        {
            bool confirm = await DisplayAlert(AppResources.DeleteBlockTitle, 
                AppResources.DeleteBlockConfirmation, 
                AppResources.Delete, AppResources.Cancel);

            if (confirm)
            {
                _blocks.Remove(block);
                UpdateSummary();
            }
        }
    }

    private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(AppResources.DeleteWorkoutTitle, 
            AppResources.DeleteWorkoutConfirmation, 
            AppResources.Delete, AppResources.Cancel);
        
        if (!confirm)
            return;
        
        try
        {
            // Delete the workout using its ID
            bool deleted = await WorkoutStorageService.DeleteWorkoutAsync(_workout.Id);
            
            if (deleted)
            {
                await DisplayAlert(AppResources.OK, AppResources.DeleteWorkoutTitle, AppResources.OK);
                
                // Send message using WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new WorkoutDeletedMessage(_workout.Id));
                
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert(AppResources.Error, AppResources.FailedToDeleteWorkout, AppResources.OK);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, string.Format(AppResources.AnErrorOccurredFormat, ex.Message), AppResources.OK);
        }
    }

    private void OnMoveUpClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WorkBlock block)
        {
            int index = _blocks.IndexOf(block);
            if (index > 0)
            {
                _blocks.Move(index, index - 1);
            }
        }
    }

    private void OnMoveDownClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WorkBlock block)
        {
            int index = _blocks.IndexOf(block);
            if (index < _blocks.Count - 1)
            {
                _blocks.Move(index, index + 1);
            }
        }
    }

    private void UpdateSummary()
    {
        TotalBlocksLabel.Text = $"{_blocks.Count} block{(_blocks.Count != 1 ? "s" : "")}";
        
        uint totalSeconds = (uint)_blocks.Sum(b => b.Duration);
        uint minutes = totalSeconds / 60;
        uint seconds = totalSeconds % 60;
        
        if (minutes >= 60)
        {
            uint hours = minutes / 60;
            minutes = minutes % 60;
            TotalDurationLabel.Text = $"{hours}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            TotalDurationLabel.Text = $"{minutes}:{seconds:D2}";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate workout name
        if (string.IsNullOrWhiteSpace(WorkoutNameEntry.Text))
        {
            await DisplayAlert(AppResources.Error, AppResources.PleaseEnterWorkoutName, AppResources.OK);
            return;
        }
        
        try
        {
            // Update workout
            _workout.Name = WorkoutNameEntry.Text;
            _workout.Blocks.Clear();
            _workout.Blocks.AddRange(_blocks);
            
            // Save using the service
            await WorkoutStorageService.SaveWorkoutAsync(_workout);
            
            // Send message using WeakReferenceMessenger
            WeakReferenceMessenger.Default.Send(new WorkoutSavedMessage(_workout));
            
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, string.Format(AppResources.FailedToSaveWorkoutFormat, ex.Message), AppResources.OK);
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(AppResources.DiscardChangesTitle, 
            AppResources.DiscardChangesMessage, 
            AppResources.Discard, AppResources.KeepEditing);
        
        if (confirm)
        {
            await Navigation.PopModalAsync();
        }
    }
}
