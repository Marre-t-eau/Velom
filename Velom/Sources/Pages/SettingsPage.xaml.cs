using Velom.Resources.Strings;
using Velom.Sources.Services;
using System.Globalization;

namespace Velom.Sources.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadCurrentLanguage();
    }

    private void LoadCurrentLanguage()
    {
        var currentCulture = LocalizationService.CurrentCulture;
        var index = LocalizationService.GetSupportedCultures()
            .ToList()
            .FindIndex(c => c.Name == currentCulture.Name);

        if (index >= 0)
        {
            LanguagePicker.SelectedIndex = index;
        }
    }

    private async void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex < 0)
            return;

        var selectedCulture = LocalizationService.GetSupportedCultures()[LanguagePicker.SelectedIndex];

        if (selectedCulture.Name != LocalizationService.CurrentCulture.Name)
        {
            LocalizationService.SetCulture(selectedCulture);

            var message = selectedCulture.TwoLetterISOLanguageName == "fr"
                ? "Veuillez redémarrer l'application pour appliquer le changement de langue."
                : "Please restart the app to apply the language change.";

            //await DisplayAlert(AppResources.AppName, message, AppResources.OK);
        }
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
