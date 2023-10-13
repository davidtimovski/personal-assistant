using Core.Application.Contracts.Models;
using FluentValidation;
using Sentry;

namespace Core.Application.Contracts;

public interface ICdnService
{
    string GetDefaultProfileImageUri();
    string GetDefaultRecipeImageUri();
    string ImageUriToThumbnail(string imageUri);
    Task<string> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task RemoveTempTagAsync(string imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteAsync(string imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task CreateFolderForUserAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteTemporaryResourcesAsync(DateTime olderThan, CancellationToken cancellationToken);
}
