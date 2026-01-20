using System.Composition;
using Velom.Sources.Objects.WorkoutHistory;

namespace Velom.Sources.Pages;

public partial class WorkoutHistoryPage : ContentPage
{
    [Import]
    private IWorkoutHistoryService HistoryService { get; init; }

    private List<WorkoutSessionViewModel> _sessions = new();

    public WorkoutHistoryPage()
    {
        InitializeComponent();
        App.Container.SatisfyImports(this);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        var sessions = await HistoryService.GetAllSessionsAsync();
        _sessions = sessions.Select(s => new WorkoutSessionViewModel(s)).ToList();
        HistoryCollectionView.ItemsSource = _sessions;
    }

    private async void OnWorkoutTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is WorkoutSessionViewModel selectedSession)
        {
            // Visual feedback
            await frame.ScaleTo(0.95, 50);
            await frame.ScaleTo(1.0, 50);
            
            await Navigation.PushAsync(new WorkoutHistoryDetailPage(selectedSession.Id));
        }
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}

/// <summary>
/// ViewModel for displaying workout session in list
/// </summary>
internal class WorkoutSessionViewModel
{
    public int Id { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string FormattedDuration { get; set; } = string.Empty;
    public double AveragePower { get; set; }
    public ushort MaxPower { get; set; }
    public double TSS { get; set; }
    public double TotalKilojoules { get; set; }

    public WorkoutSessionViewModel(WorkoutSession session)
    {
        Id = session.Id;
        WorkoutName = session.WorkoutName;
        StartTime = session.StartTime;
        AveragePower = session.AveragePower;
        MaxPower = session.MaxPower;
        TSS = session.TSS;
        TotalKilojoules = session.TotalKilojoules;

        // Format duration as HH:MM:SS
        var duration = TimeSpan.FromSeconds(session.TotalDurationSeconds);
        FormattedDuration = $"⏱️ {duration:hh\\:mm\\:ss}";
    }
}
