using System.Collections.Generic;

namespace Account.ViewModels.Home;

public enum OverviewAlert
{
    None,
    ProfileUpdated,
    PasswordResetEmailSent
}

public class OverviewViewModel
{
    public string UserName { get; set; }
    public List<ClientApplicationViewModel> ClientApplications { get; set; }
    public OverviewAlert Alert { get; set; }
}
