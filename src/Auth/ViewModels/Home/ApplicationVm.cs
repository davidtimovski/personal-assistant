using System;

namespace Auth.ViewModels.Home;

public class ApplicationVm
{
    public ApplicationVm(string name, Uri url, string cssClass) : this(name, url, cssClass, false) { }
    public ApplicationVm(string name, Uri url, string cssClass, bool inDevelopment)
    {
        Name = name;
        Url = url;
        CssClass = cssClass;
        InDevelopment = inDevelopment;
    }

    public string Name { get; set; }
    public Uri Url { get; set; }
    public string CssClass { get; set; }
    public bool InDevelopment { get; set; }
}