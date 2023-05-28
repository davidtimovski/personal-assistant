using System.ComponentModel.DataAnnotations;

namespace Jobs;

public class JobsConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;

#if !DEBUG
    [Required]
#endif
    public string FixerApiAccessKey { get; set; } = null!;
}
