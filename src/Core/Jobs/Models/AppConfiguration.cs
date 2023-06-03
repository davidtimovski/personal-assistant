﻿using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;

namespace Jobs.Models;

public class AppConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;

#if !DEBUG
    [Required]
#endif
    public string FixerApiAccessKey { get; set; } = null!;

    [Required]
    public CloudinaryConfig Cloudinary { get; set; } = null!;
}
