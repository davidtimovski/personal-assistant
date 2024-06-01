namespace PersonalAssistant.Web.ViewModels.Home;

public enum OverviewIndexAlert
{
    None,
    LoggedOut,
    SuccessfullyRegistered,
    LanguageChanged,
    AccountDeleted
}

public class IndexViewModel
{
    public IndexViewModel(OverviewIndexAlert alert)
    {
        Alert = alert;
    }

    public OverviewIndexAlert Alert { get; }
}
