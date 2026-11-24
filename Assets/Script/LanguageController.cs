using UnityEngine;
using UnityEngine.Localization.Settings;
public class LanguageController : MonoBehaviour
{
    public void SelectLanguage(string LanguageCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(LanguageCode);
        LocalizationSettings.SelectedLocale = locale;
    }
    
}
