﻿using Application.Contracts.Models;
using FluentValidation;

namespace Application.Contracts;

public interface ICdnService
{
    string GetDefaultProfileImageUri();
    string GetDefaultRecipeImageUri();
    string ImageUriToThumbnail(string imageUri);
    Task<string> UploadAsync(string filePath, string uploadPath, string template);
    Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator);
    Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template);
    Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template);
    Task RemoveTempTagAsync(string imageUri);
    Task DeleteAsync(string imageUri);
    Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris);
    Task DeleteTemporaryResourcesAsync(DateTime olderThan);
}