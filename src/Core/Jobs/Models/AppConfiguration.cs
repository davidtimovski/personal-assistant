using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;

namespace Jobs.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public FixerApiConfig FixerApi { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public CloudinaryConfig Cloudinary { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.Production.json.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public DbBackupConfig DbBackup { get; set; } = null!;
}

public class FixerApiConfig
{
    [Required]
    public string BaseUrl { get; set; } = null!;

#if !DEBUG
    [Required]
#endif
    public string AccessKey { get; set; } = null!;
}

public class DbBackupConfig
{
#if !DEBUG
    [Required]
#endif
    public string AzureStorageConnectionString { get; set; } = null!;

    [Required]
    public string AzureStorageContainerName { get; set; } = null!;

    [Required]
    public string BackupsPath { get; set; } = null!;
}
