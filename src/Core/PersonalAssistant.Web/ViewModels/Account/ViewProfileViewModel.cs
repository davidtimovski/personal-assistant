using System.Globalization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class ViewProfileViewModel
{
    public ViewProfileViewModel(string name, string? country, string language, string culture, string imageUri, string defaultImageUri, string baseUrl)
    {
        Name = name;
        Country = country;
        Language = language;
        Culture = culture;
        ImageUri = imageUri;
        DefaultImageUri = defaultImageUri;
        BaseUrl = baseUrl;

        CultureOptions = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .OrderBy(x => x.EnglishName)
            .Select(x => new CultureOption(x.Name, x.EnglishName)).ToList();
    }

    public string Name { get; }
    public string? Country { get; }
    public string Language { get; }
    public string Culture { get; }
    public string ImageUri { get; }
    public string DefaultImageUri { get; }
    public string BaseUrl { get; }
    public List<CultureOption> CultureOptions { get; }
}
