namespace Account.Web.ViewModels.Home;

public enum ReleaseStatus
{
    Released,
    Beta,
    InDevelopment
}

public class ClientApplicationViewModel
{
    public ClientApplicationViewModel(string name, string cssClass)
    {
        Name = name;
        CssClass = cssClass;
        ReleaseStatus = ReleaseStatus.InDevelopment;
    }

    public ClientApplicationViewModel(string name, string url, string cssClass, ReleaseStatus releaseStatus = ReleaseStatus.Released)
    {
        Name = name;
        Url = url;
        CssClass = cssClass;
        ReleaseStatus = releaseStatus;
    }

    public string Name { get; }
    public string Url { get; }
    public string CssClass { get; }
    public ReleaseStatus ReleaseStatus { get; }
}
