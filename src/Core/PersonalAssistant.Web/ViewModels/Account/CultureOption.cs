namespace PersonalAssistant.Web.ViewModels.Account;

public class CultureOption
{
    public CultureOption(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }
    public string Label { get; }
}
