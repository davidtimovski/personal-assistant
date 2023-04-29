using Core.Application.Contracts.Models;
using FluentValidation;
using Sentry;

namespace Core.Application.Contracts;

public interface ICdnService
{
    string GetDefaultProfileImageUri();
    string GetDefaultRecipeImageUri();
    string ImageUriToThumbnail(string imageUri);
    Task<string> UploadAsync(string filePath, string uploadPath, string template, ITransaction tr);
    Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ITransaction tr);
    Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template, ITransaction tr);
    Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template, ITransaction tr);
    Task RemoveTempTagAsync(string imageUri, ITransaction tr);
    Task DeleteAsync(string imageUri, ITransaction tr);
    Task CreateFolderForUserAsync(int userId, ITransaction tr);
    Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris, ITransaction tr);
    Task DeleteTemporaryResourcesAsync(DateTime olderThan);
}
