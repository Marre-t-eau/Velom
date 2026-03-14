using CommunityToolkit.Mvvm.Messaging;
using Velom.Sources.Messages;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Objects.Workout.View;
using Velom.Sources.Services;
using Velom.Sources.Objects;
using System.Collections.ObjectModel;
using Velom.Resources.Strings;

namespace Velom.Sources.Pages;

public partial class WorkoutsListPage : ContentPage
{
    private List<Workout> Workouts { get; set; } = [];
    private ObservableCollection<WorkoutView> WorkoutViews { get; set; } = [];

    public WorkoutsListPage()
	{
		InitializeComponent();
        
        // Set ItemsSource once at initialization
        WorkoutsCollectionView.ItemsSource = WorkoutViews;
        
        // Register as recipient for workout messages
        WeakReferenceMessenger.Default.Register<WorkoutSavedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ReloadWorkoutsAsync();
            });
        });
        
        WeakReferenceMessenger.Default.Register<WorkoutDeletedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ReloadWorkoutsAsync();
            });
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload workouts when page appears (e.g., after returning from editor)
        _ = ReloadWorkoutsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unregister from all messages
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private async Task LoadWorkoutsAsync()
    {
        try
        {
            WorkoutViews.Clear();
            
            // Get user FTP
            var userInfo = await UserInfo.GetUserInfo();
            ushort userFTP = userInfo.FTP;
            
            // Load workouts
            Workouts = await WorkoutStorageService.LoadWorkoutsAsync();
            
            foreach (var workout in Workouts)
            {
                WorkoutView workoutView = new WorkoutView(workout);
                workoutView.FTP = userFTP;
                WorkoutViews.Add(workoutView);
            }
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert(AppResources.Error, string.Format(AppResources.FailedToLoadWorkoutsFormat, ex.Message), AppResources.OK);
            });
        }
    }

    private async Task ReloadWorkoutsAsync()
    {
        await LoadWorkoutsAsync();
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WorkoutView workout)
        {
            await Navigation.PushModalAsync(new WorkoutPage(workout));
        }
    }

    private async void OnEditWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WorkoutView workout)
        {
            // Edit workout
            // Find the original workout by Id without the modifications of FTP from WorkoutView
            await Navigation.PushModalAsync(new WorkoutEditorPage(Workouts.Find(w => w.Id == workout.Id) ?? new Workout()));
        }
    }

    private async void OnNewWorkoutClicked(object sender, EventArgs e)
    {
        var newWorkout = new Workout { Name = "New Workout" };
        await Navigation.PushModalAsync(new WorkoutEditorPage(newWorkout));
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}