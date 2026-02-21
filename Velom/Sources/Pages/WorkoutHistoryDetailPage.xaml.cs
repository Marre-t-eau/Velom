using System.Composition;
using Velom.Sources.Objects.WorkoutHistory;
using Velom.Sources.Services;

namespace Velom.Sources.Pages;

public partial class WorkoutHistoryDetailPage : ContentPage
{
    [Import]
    private IWorkoutHistoryService HistoryService { get; init; }

    private int _sessionId;
    private WorkoutSession? _session;
    private WorkoutExportService _exportService = new();

    public WorkoutHistoryDetailPage(int sessionId)
    {
        InitializeComponent();
        App.Container.SatisfyImports(this);
        _sessionId = sessionId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkoutDetails();
    }

    private async Task LoadWorkoutDetails()
    {
        _session = await HistoryService.GetSessionAsync(_sessionId);
        
        if (_session == null)
        {
            await DisplayAlert("Error", "Workout session not found", "OK");
            await Navigation.PopAsync();
            return;
        }

        // Header Info
        WorkoutNameLabel.Text = _session.WorkoutName;
        DateLabel.Text = $"{_session.StartTime:dddd, MMMM dd, yyyy 'at' HH:mm}";
        
        var duration = TimeSpan.FromSeconds(_session.TotalDurationSeconds);
        DurationLabel.Text = $"Duration: {duration:hh\\:mm\\:ss}";
        
        CompletedLabel.Text = _session.IsCompleted 
            ? "Completed" 
            : "Stopped early";
        CompletedLabel.TextColor = _session.IsCompleted 
            ? Colors.Green 
            : Colors.Orange;

        // Key Metrics
        AvgPowerLabel.Text = $"{_session.AveragePower:F0} W";
        MaxPowerLabel.Text = $"{_session.MaxPower} W";
        AvgCadenceLabel.Text = _session.AverageCadence > 0 
            ? $"{_session.AverageCadence:F0} rpm" 
            : "N/A";
        AvgHRLabel.Text = _session.AverageHeartRate > 0 
            ? $"{_session.AverageHeartRate:F0} bpm" 
            : "N/A";
        EnergyLabel.Text = $"{_session.TotalKilojoules:F0} kJ";
        TSSLabel.Text = $"{_session.TSS:F0}";

        // Advanced Metrics
        NormalizedPowerLabel.Text = $"Normalized Power (NP): {_session.NormalizedPower:F0} W";
        IntensityFactorLabel.Text = $"Intensity Factor (IF): {_session.IntensityFactor:F2}";
        FTPLabel.Text = $"FTP: {_session.FTP} W";

        // Notes
        NotesEditor.Text = _session.Notes;
    }

    private async void OnNotesChanged(object sender, TextChangedEventArgs e)
    {
        if (_session != null)
        {
            _session.Notes = e.NewTextValue;
            await HistoryService.UpdateSessionAsync(_session);
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_session == null)
            return;

        try
        {
            var records = await HistoryService.GetSessionRecordsAsync(_sessionId);
            bool success = await _exportService.ShareTcxFileAsync(_session, records);

            if (!success)
            {
                await DisplayAlert("Error", "Failed to export workout", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Delete Workout", 
            "Are you sure you want to delete this workout? This action cannot be undone.", 
            "Delete", 
            "Cancel");

        if (confirm)
        {
            await HistoryService.DeleteSessionAsync(_sessionId);
            await Navigation.PopAsync();
        }
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        // Pop to root (close all modals and go back to main page)
        await Navigation.PopToRootAsync();
    }
}

