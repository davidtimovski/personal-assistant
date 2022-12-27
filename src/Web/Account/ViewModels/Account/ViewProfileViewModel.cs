using System.Globalization;

namespace Account.ViewModels.Account;

public class ViewProfileViewModel
{
    public ViewProfileViewModel()
    {
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
