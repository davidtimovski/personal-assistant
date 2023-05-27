using Core.Application.Contracts.Models;
using FluentValidation;
using Sentry;

namespace Core.Application.Contracts;

public interface ICdnService
{
    string GetDefaultProfileImageUri();
    string GetDefaultRecipeImageUri();
    string ImageUriToThumbnail(string imageUri);
    Task<string> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan);
    Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan);
    Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan);
    Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template, ISpan metricsSpan);
    Task RemoveTempTagAsync(string imageUri, ISpan metricsSpan);
    Task DeleteAsync(string imageUri, ISpan metricsSpan);
    Task CreateFolderForUserAsync(int userId, ISpan metricsSpan);
    Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris, ISpan metricsSpan);
    Task DeleteTemporaryResourcesAsync(DateTime olderThan);
}
