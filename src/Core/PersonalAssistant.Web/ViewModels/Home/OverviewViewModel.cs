namespace PersonalAssistant.Web.ViewModels.Home;

public enum OverviewAlert
{
    None,
    ProfileUpdated,
    PasswordResetEmailSent
}

public class OverviewViewModel
{
    public OverviewViewModel(string userName, List<ClientApplicationViewModel> clientApplications, OverviewAlert alert)
    {
        UserName = userName;
        ClientApplications = clientApplications;
        Alert = alert;
    }

    public string UserName { get; set; }
    public List<ClientApplicationViewModel> ClientApplications { get; set; }
    public OverviewAlert Alert { get; set; }
}
