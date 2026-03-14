using System.Globalization;

namespace Velom.Sources.Services;

public class LocalizationService
{
    private const string LanguagePreferenceKey = "app_language";
    
    private static readonly List<CultureInfo> SupportedCultures = new()
    {
        new CultureInfo("en"),
        new CultureInfo("fr")
    };
    
    public static CultureInfo CurrentCulture { get; private set; }
    
    public static IReadOnlyList<CultureInfo> GetSupportedCultures() => SupportedCultures.AsReadOnly();
    
    public static void Initialize()
    {
        var savedLanguage = Preferences.Get(LanguagePreferenceKey, string.Empty);
        
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            var culture = new CultureInfo(savedLanguage);
            SetCulture(culture);
        }
        else
        {
            var systemCulture = CultureInfo.CurrentUICulture;
            var matchingCulture = SupportedCultures.FirstOrDefault(c => 
                c.TwoLetterISOLanguageName == systemCulture.TwoLetterISOLanguageName) 
                ?? SupportedCultures[0];
            
            SetCulture(matchingCulture);
        }
    }
    
    public static void SetCulture(CultureInfo culture)
    {
        if (!SupportedCultures.Any(c => c.Name == culture.Name))
        {
            culture = SupportedCultures[0];
        }
        
        CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        Resources.Strings.LocalizationResourceManager.SetCulture(culture);
        
        Preferences.Set(LanguagePreferenceKey, culture.Name);
    }
    
    public static string GetLanguageName(CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "fr" => "Français",
            "en" => "English",
            _ => culture.DisplayName
        };
    }
}
