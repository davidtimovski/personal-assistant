namespace Cdn.Configuration;

public class CloudinaryConfig
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
    public DefaultImageUris DefaultImageUris { get; set; } = null!;
}
