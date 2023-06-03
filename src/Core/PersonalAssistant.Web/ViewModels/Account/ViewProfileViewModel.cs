using System.Globalization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class ViewProfileViewModel
{
    public ViewProfileViewModel(string name, string language, string culture, string imageUri, string defaultImageUri, string baseUrl)
    {
        Name = name;
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

    public string Name { get; set; }
    public string Language { get; set; }
    public string Culture { get; set; }
    public string ImageUri { get; set; }
    public string DefaultImageUri { get; set; }
    public string BaseUrl { get; set; }
    public List<CultureOption> CultureOptions { get; }
}
