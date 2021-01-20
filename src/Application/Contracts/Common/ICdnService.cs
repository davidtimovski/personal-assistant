using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ICdnService
    {
        string GetDefaultProfileImageUri();
        string GetDefaultRecipeImageUri();
        string ImageUriToThumbnail(string imageUri);
        Task<string> UploadAsync(string filePath, string uploadPath, string template);
        Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator);
        Task<string> UploadProfileTempAsync(int userId, string filePath, string uploadPath, string template);
        Task<string> CopyAndUploadAsync(string tempImagePath, string imageUriToCopy, string uploadPath, string template);
        Task RemoveTempTagAsync(string imageUri);
        Task DeleteAsync(string imageUri);
        Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris);
        Task DeleteTemporaryResourcesAsync(DateTime olderThan);
    }
}
