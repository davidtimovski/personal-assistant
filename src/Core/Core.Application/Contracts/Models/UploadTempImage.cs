﻿using FluentValidation;

namespace Core.Application.Contracts.Models;

public class UploadTempImage
{
    public UploadTempImage(int userId, string localTempPath, string uploadPath, string transformationTemplate, long length, string fileName)
    {
        UserId = userId;
        LocalTempPath = localTempPath;
        UploadPath = uploadPath;
        TransformationTemplate = transformationTemplate;
        Length = length;
        FileName = fileName;
    }

    public int UserId { get; set; }
    public string LocalTempPath { get; set; }
    public string UploadPath { get; set; }
    public string TransformationTemplate { get; set; }
    public long Length { get; set; }
    public string FileName { get; set; }
    public MemoryStream File { get; set; } = new MemoryStream();
}

public class UploadTempImageValidator : AbstractValidator<UploadTempImage>
{
    private static readonly HashSet<string> ValidFormats = new() { ".JPG", ".PNG", ".JPEG" };

    public UploadTempImageValidator()
    {
        RuleFor(dto => dto.UserId).NotEmpty().WithMessage("Unauthorized");

        const int maxSizeInMegabytes = 10;
        RuleFor(dto => dto.Length)
            .NotEmpty().WithMessage("Common.ImageIsEmpty")
            .LessThanOrEqualTo(maxSizeInMegabytes * 1024 * 1024).WithMessage("Common.ImageIsTooLarge");

        RuleFor(dto => dto.FileName)
            .Must(fileName =>
            {
                string extension = Path.GetExtension(fileName);
                return ValidFormats.Contains(extension.ToUpperInvariant());
            }).WithMessage("Common.InvalidImageFormat");
    }
}
