namespace Account.Web.ViewModels.Home;

public enum IndexAlert
{
    None,
    LoggedOut,
    SuccessfullyRegistered,
    LanguageChanged,
    AccountDeleted
}

public class IndexViewModel
{
    public IndexViewModel(IndexAlert alert)
    {
        Alert = alert;
    }

    public IndexAlert Alert { get; }
}
