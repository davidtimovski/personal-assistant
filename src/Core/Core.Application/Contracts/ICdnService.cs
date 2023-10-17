using Core.Application.Contracts.Models;
using FluentValidation;
using Sentry;

namespace Core.Application.Contracts;

public interface ICdnService
{
    public Uri DefaultProfileImageUri { get; }
    public Uri DefaultRecipeImageUri { get; }

    Uri ImageUriToThumbnail(Uri imageUri);
    Task<Result<Uri>> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult<string>> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result<Uri>> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result<Uri>> CopyAndUploadAsync(string localTempPath, Uri imageUriToCopy, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> RemoveTempTagAsync(Uri imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Uri imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> CreateFolderForUserAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> DeleteUserResourcesAsync(int userId, IEnumerable<Uri> imageUris, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> DeleteTemporaryResourcesAsync(DateTime olderThan, CancellationToken cancellationToken);
}
