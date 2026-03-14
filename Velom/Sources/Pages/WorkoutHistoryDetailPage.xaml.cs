using System.Composition;
using Velom.Sources.Objects.WorkoutHistory;
using Velom.Sources.Services;
using Velom.Resources.Strings;

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
            await DisplayAlert(AppResources.Error, AppResources.WorkoutSessionNotFound, AppResources.OK);
            await Navigation.PopAsync();
            return;
        }

        // Header Info
        WorkoutNameLabel.Text = _session.WorkoutName;
        DateLabel.Text = $"{_session.StartTime:dddd, MMMM dd, yyyy 'at' HH:mm}";
        
        var duration = TimeSpan.FromSeconds(_session.TotalDurationSeconds);
        DurationLabel.Text = string.Format(AppResources.DurationFormat, duration.ToString(@"hh\:mm\:ss"));
        
        CompletedLabel.Text = _session.IsCompleted 
            ? AppResources.Completed 
            : AppResources.StoppedEarly;
        CompletedLabel.TextColor = _session.IsCompleted 
            ? Colors.Green 
            : Colors.Orange;

        // Key Metrics
        AvgPowerLabel.Text = $"{_session.AveragePower:F0} W";
        MaxPowerLabel.Text = $"{_session.MaxPower} W";
        AvgCadenceLabel.Text = _session.AverageCadence > 0 
            ? $"{_session.AverageCadence:F0} rpm" 
            : AppResources.NA;
        AvgHRLabel.Text = _session.AverageHeartRate > 0 
            ? $"{_session.AverageHeartRate:F0} bpm" 
            : AppResources.NA;
        EnergyLabel.Text = $"{_session.TotalKilojoules:F0} kJ";
        TSSLabel.Text = $"{_session.TSS:F0}";

        // Advanced Metrics
        NormalizedPowerLabel.Text = string.Format(AppResources.NormalizedPowerFormat, _session.NormalizedPower);
        IntensityFactorLabel.Text = string.Format(AppResources.IntensityFactorFormat, _session.IntensityFactor);
        FTPLabel.Text = string.Format(AppResources.FTPFormat, _session.FTP);

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
                await DisplayAlert(AppResources.Error, AppResources.FailedToExportWorkout, AppResources.OK);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, string.Format(AppResources.FailedToExportFormat, ex.Message), AppResources.OK);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            AppResources.DeleteWorkoutTitle, 
            AppResources.DeleteWorkoutConfirmation, 
            AppResources.Delete, 
            AppResources.Cancel);

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

