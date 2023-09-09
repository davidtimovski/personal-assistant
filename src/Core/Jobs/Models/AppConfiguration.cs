using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;

namespace Jobs.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required FixerApiConfig FixerApi { get; set; }

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public required CloudinaryConfig Cloudinary { get; set; }

    /// <summary>
    /// Coming from appsettings.Production.json.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required DbBackupConfig DbBackup { get; set; }
}

public class FixerApiConfig
{
    [Required]
    public required string BaseUrl { get; set; }

#if !DEBUG
    [Required]
#endif
    public required string AccessKey { get; set; }
}

public class DbBackupConfig
{
#if !DEBUG
    [Required]
#endif
    public required string AzureStorageConnectionString { get; set; }

    [Required]
    public required string AzureStorageContainerName { get; set; }

    [Required]
    public required string BackupsPath { get; set; }
}
