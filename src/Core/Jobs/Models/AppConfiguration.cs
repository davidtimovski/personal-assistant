using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;

namespace Jobs.Models;

public sealed class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required string JobsConnectionString { get; init; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required FixerApiConfig FixerApi { get; init; }

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public required CloudinaryConfig Cloudinary { get; init; }

    /// <summary>
    /// Coming from appsettings.Production.json.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required DbBackupConfig DbBackup { get; init; }
}

public sealed class FixerApiConfig
{
    [Required]
    public required string BaseUrl { get; init; }

#if !DEBUG
    [Required]
#endif
    public required string AccessKey { get; init; }
}

public sealed class DbBackupConfig
{
#if !DEBUG
    [Required]
#endif
    public required string AzureStorageConnectionString { get; init; }

    [Required]
    public required string AzureStorageContainerName { get; init; }

    [Required]
    public required string BackupsPath { get; init; }
}
