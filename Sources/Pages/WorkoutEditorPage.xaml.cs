using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Velom.Sources.Messages;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Services;

namespace Velom.Sources.Pages;

public partial class WorkoutEditorPage : ContentPage
{
    private Workout _workout;
    private ObservableCollection<WorkBlock> _blocks;
    private bool _isWattsMode = true; // true = Watts, false = % FTP

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
            bool confirm = await DisplayAlert("Delete Block?", 
                "Are you sure you want to delete this block?", 
                "Delete", "Cancel");
            
            if (confirm)
            {
                _blocks.Remove(block);
                UpdateSummary();
            }
        }
    }

    private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Delete Workout?", 
            $"Are you sure you want to delete '{WorkoutNameEntry.Text}'? This action cannot be undone.", 
            "Delete", "Cancel");
        
        if (!confirm)
            return;
        
        try
        {
            // Delete the workout using its ID
            bool deleted = await WorkoutStorageService.DeleteWorkoutAsync(_workout.Id);
            
            if (deleted)
            {
                await DisplayAlert("Success", "Workout deleted successfully", "OK");
                
                // Send message using WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new WorkoutDeletedMessage(_workout.Id));
                
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete workout", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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
            await DisplayAlert("Error", "Please enter a workout name", "OK");
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
            await DisplayAlert("Error", $"Failed to save workout: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Discard Changes?", 
            "Are you sure you want to discard your changes?", 
            "Discard", "Keep Editing");
        
        if (confirm)
        {
            await Navigation.PopModalAsync();
        }
    }
}
