using System.Reflection;
using System.Text.Json;
using Velom.Sources.Objects.Workout;
using Velom.Sources.Objects.Workout.View;

namespace Velom.Sources.Pages;

public partial class WorkoutsListPage : ContentPage
{
	private List<Workout> Workouts { get; } = [];
    private List<WorkoutView> WorkoutViews { get; } = [];

    public WorkoutsListPage()
	{
		InitializeComponent();
        LoadWorkouts();
        WorkoutsCollectionView.ItemsSource = WorkoutViews;
    }

    private async void LoadWorkouts()
    {
        Assembly assembly = typeof(WorkoutsListPage).Assembly;
        IEnumerable<string> resourceNames = assembly.GetManifestResourceNames().Where(r => r.Contains("Workouts") && r.EndsWith(".json"));
        foreach (string resourceName in resourceNames)
        {
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            Workout? workout = JsonSerializer.Deserialize<Workout>(json);
            if (workout != null)
            {
                Workouts.Add(workout);
                WorkoutViews.Add(new WorkoutView(workout));
            }
        }
    }

    private async void OnWorkoutSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Workout selectedWorkout)
        {
            await Navigation.PushModalAsync(new WorkoutPage(new Workout(selectedWorkout)));
        }
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}