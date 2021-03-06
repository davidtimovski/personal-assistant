﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;

namespace PersonalAssistant.Application.Contracts.Common.Models
{
    public class UploadTempImage
    {
        public UploadTempImage(int userId, string localTempPath, string uploadPath, string transformationTemplate)
        {
            UserId = userId;
            LocalTempPath = localTempPath;
            UploadPath = uploadPath;
            TransformationTemplate = transformationTemplate;
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
        public UploadTempImageValidator()
        {
            RuleFor(dto => dto.UserId).NotEmpty().WithMessage("Unauthorized");

            RuleFor(dto => dto.Length)
                .NotEmpty().WithMessage("Common.ImageIsEmpty")
                .LessThan(10 * 1024 * 1024).WithMessage("Common.ImageTooLarge");

            RuleFor(dto => dto.FileName)
                .Must(fileName =>
                {
                    string extension = Path.GetExtension(fileName);

                    if (!new string[] { ".JPG", ".PNG", ".JPEG" }.Contains(extension.ToUpperInvariant()))
                    {
                        return false;
                    }
                    return true;
                }).WithMessage("Common.InvalidImageFormat");
        }
    }
}
