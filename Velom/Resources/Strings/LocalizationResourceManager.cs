using System.Globalization;

namespace Velom.Resources.Strings;

public static class LocalizationResourceManager
{
    public static void SetCulture(CultureInfo culture)
    {
        AppResources.Culture = culture;
    }
}

