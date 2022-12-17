namespace Account.ViewModels.Home;

public enum IndexAlert
{
    None,
    LoggedOut,
    SuccessfullyRegistered,
    LanguageChanged
}

public class IndexViewModel
{
    public IndexViewModel(IndexAlert alert)
    {
        Alert = alert;
    }

    public IndexAlert Alert { get; }
}
