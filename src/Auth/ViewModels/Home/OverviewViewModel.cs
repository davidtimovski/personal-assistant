using System.Collections.Generic;

namespace Auth.ViewModels.Home;

public enum OverviewAlert
{
    None,
    ProfileUpdated,
    PasswordChanged
}

public class OverviewViewModel
{
    public string UserName { get; set; }
    public List<ClientApplicationViewModel> ClientApplications { get; set; }
    public OverviewAlert Alert { get; set; }
}
