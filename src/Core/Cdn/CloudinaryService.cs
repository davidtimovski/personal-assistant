using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Cdn;

public class CloudinaryService : ICdnService
{
    private const string Format = "webp";

    private readonly string _environment;
    private readonly string _baseUrl;
    private readonly IReadOnlyDictionary<string, Transformation> _templates = new Dictionary<string, Transformation>
    {
        { "profile", new Transformation().Width(400).Height(400).Crop("lfill").FetchFormat(Format) },
        { "recipe", new Transformation().Width(640).Height(320).Crop("lfill").FetchFormat(Format) }
    };
    private readonly HttpClient _httpClient;
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(
        Account cloudinaryAccount,
        string environment,
        Uri defaultProfileImageUri,
        Uri defaultRecipeImageUri,
        HttpClient httpClient,
        ILogger<CloudinaryService> logger)
    {
        _environment = environment.ToLowerInvariant();
        DefaultProfileImageUri = defaultProfileImageUri;
        DefaultRecipeImageUri = defaultRecipeImageUri;
        _baseUrl = $"https://res.cloudinary.com/personalassistant/{_environment}/";
        _httpClient = httpClient;
        _cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryAccount);
        _logger = logger;
    }

    public Uri DefaultProfileImageUri { get; private set; }
    public Uri DefaultRecipeImageUri { get; private set; }

    public Uri ImageUriToThumbnail(Uri imageUri)
    {
        string[] parts = imageUri.ToString().Split("personalassistant");
        return new Uri(parts[0] + "personalassistant/w_80,h_80,c_limit" + parts[1]);
    }

    public async Task<Result<Uri>> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadAsync)}");

        try
        {
            string uri = GenerateRandomString();
            Transformation transformation = _templates[template];

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_environment}/{uploadPath}/",
                PublicId = uri,
                Transformation = transformation
            };

            ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (uploadResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(UploadAsync)}() returned error: {uploadResult.Error.Message}");
            }

            File.Delete(filePath);

            var url = new Uri(_baseUrl + $"{uploadPath}/{uri}.{Format}");

            return new(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UploadAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult<string>> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadTempAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            string extension = Path.GetExtension(model.FileName);

            if (!Directory.Exists(model.LocalTempPath))
            {
                Directory.CreateDirectory(model.LocalTempPath);
            }

            string tempImagePath = Path.Combine(model.LocalTempPath, Guid.NewGuid() + extension);
            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            {
                model.File.Position = 0;
                await model.File.CopyToAsync(stream, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            string imageUri = await UploadTempAsync(
                filePath: tempImagePath,
                uploadPath: model.UploadPath,
                template: model.TransformationTemplate,
                cancellationToken
            );

            return new(imageUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UploadTempAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result<Uri>> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadProfileTempAsync)}");

        try
        {
            string uri = GenerateRandomString();
            Transformation transformation = _templates[template];

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_environment}/{uploadPath}/",
                PublicId = uri,
                Transformation = transformation,
                Tags = "temp"
            };

            ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (uploadResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(UploadProfileTempAsync)}() returned error: {uploadResult.Error.Message}");
            }

            File.Delete(filePath);

            var url = new Uri(_baseUrl + $"{uploadPath}/{uri}.{Format}");

            return new(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UploadProfileTempAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result<Uri>> CopyAndUploadAsync(string localTempPath, Uri imageUriToCopy, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (imageUriToCopy.Equals(DefaultRecipeImageUri))
        {
            return new(imageUriToCopy);
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CopyAndUploadAsync)}");

        try
        {
            var result = await _httpClient.GetAsync(imageUriToCopy, cancellationToken);

            if (!Directory.Exists(localTempPath))
            {
                Directory.CreateDirectory(localTempPath);
            }

            string tempImagePath = Path.Combine(localTempPath, Guid.NewGuid().ToString());

            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            using (Stream image = await result.Content.ReadAsStreamAsync(cancellationToken))
            {
                await image.CopyToAsync(stream, cancellationToken);
            }

            var imageUriResult = await UploadAsync(
                filePath: tempImagePath,
                uploadPath: uploadPath,
                template: template,
                metricsSpan: metric,
                cancellationToken
            );
            if (imageUriResult.Failed)
            {
                return new();
            }

            return new(imageUriResult.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CopyAndUploadAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> RemoveTempTagAsync(Uri imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (IsDefaultImage(imageUri))
        {
            return new(true);
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(RemoveTempTagAsync)}");

        try
        {
            string publicId = GetPublicIdFromUri(imageUri);

            var tagParams = new TagParams
            {
                PublicIds = new List<string> { publicId },
                Tag = "temp",
                Command = TagCommand.Remove
            };

            TagResult tagResult = await _cloudinary.TagAsync(tagParams, cancellationToken);
            if (tagResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(RemoveTempTagAsync)}() returned error: {tagResult.Error.Message}");
            }

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(RemoveTempTagAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> DeleteAsync(Uri imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (IsDefaultImage(imageUri))
        {
            return new(true);
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}");

        try
        {
            string publicId = GetPublicIdFromUri(imageUri);

            DelResResult deleteResult = await _cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = new List<string> { publicId }
            }, cancellationToken);

            if (deleteResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}() returned error: {deleteResult.Error.Message}");
            }

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> CreateFolderForUserAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}");

        try
        {
            CreateFolderResult result = await _cloudinary.CreateFolderAsync($"{_environment}/users/{userId}", cancellationToken);
            if (result.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}() returned error: {result.Error.Message}");
            }

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateFolderForUserAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> DeleteUserResourcesAsync(int userId, IEnumerable<Uri> imageUris, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}");

        try
        {
            var publicIds = new List<string>();
            foreach (var uri in imageUris.Where(x => !IsDefaultImage(x)))
            {
                string publicId = GetPublicIdFromUri(uri);
                publicIds.Add(publicId);
            }

            DelResResult deleteResult = await _cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = publicIds
            }, cancellationToken);
            if (deleteResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}() returned error: {deleteResult.Error.Message}");
            }

            DeleteFolderResult deleteFolderResult = await _cloudinary.DeleteFolderAsync($"{_environment}/users/{userId}", cancellationToken);
            if (deleteFolderResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}() returned error: {deleteFolderResult.Error.Message}");
            }

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteUserResourcesAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> DeleteTemporaryResourcesAsync(DateTime olderThan, CancellationToken cancellationToken)
    {
        try
        {
            var searchResult = _cloudinary.ListResourcesByTag("temp");
            if (!searchResult.Resources.Any())
            {
                return new(true);
            }

            var publicIds = searchResult.Resources.Where(x => DateTime.Parse(x.CreatedAt) < olderThan).Select(x => x.PublicId).ToList();
            if (!publicIds.Any())
            {
                return new(true);
            }

            DelResResult deleteResult = await _cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = publicIds
            }, cancellationToken);

            if (deleteResult.Error != null)
            {
                throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteTemporaryResourcesAsync)}() returned error: {deleteResult.Error.Message}");
            }

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteTemporaryResourcesAsync)}");
            return new();
        }
    }

    private bool IsDefaultImage(Uri uri)
    {
        var defaults = new[] { DefaultProfileImageUri, DefaultRecipeImageUri };
        return defaults.Contains(uri);
    }

    private async Task<string> UploadTempAsync(string filePath, string uploadPath, string template, CancellationToken cancellationToken)
    {
        string uri = GenerateRandomString();
        Transformation transformation = _templates[template];

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(filePath),
            Folder = $"{_environment}/{uploadPath}/",
            PublicId = uri,
            Transformation = transformation,
            Tags = "temp"
        };

        ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (uploadResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(UploadTempAsync)}() returned error: {uploadResult.Error.Message}");
        }

        File.Delete(filePath);

        return _baseUrl + $"{uploadPath}/{uri}.{Format}";
    }

    private string GetPublicIdFromUri(Uri uri)
    {
        var pathRegex = new Regex(@$"personalassistant\/(.*).{Format}");
        Match match = pathRegex.Match(uri.ToString());
        return match.Groups[1].Value;
    }

    private static string GenerateRandomString()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();

        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
