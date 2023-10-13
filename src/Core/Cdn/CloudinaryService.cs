using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
using FluentValidation;
using Sentry;

namespace Cdn;

public class CloudinaryService : ICdnService
{
    private const string Format = "webp";

    private readonly string _environment;
    private readonly string _defaultProfileImageUri;
    private readonly string _defaultRecipeImageUri;
    private readonly string _baseUrl;
    private readonly IReadOnlyDictionary<string, Transformation> _templates = new Dictionary<string, Transformation>
    {
        { "profile", new Transformation().Width(400).Height(400).Crop("lfill").FetchFormat(Format) },
        { "recipe", new Transformation().Width(640).Height(320).Crop("lfill").FetchFormat(Format) }
    };
    private readonly HttpClient _httpClient;
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(
        Account cloudinaryAccount,
        string environment,
        string defaultProfileImageUri,
        string defaultRecipeImageUri,
        HttpClient httpClient)
    {
        _environment = environment.ToLowerInvariant();
        _defaultProfileImageUri = defaultProfileImageUri;
        _defaultRecipeImageUri = defaultRecipeImageUri;
        _baseUrl = $"https://res.cloudinary.com/personalassistant/{_environment}/";
        _httpClient = httpClient;
        _cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryAccount);
    }

    public string GetDefaultProfileImageUri() => _defaultProfileImageUri;

    public string GetDefaultRecipeImageUri() => _defaultRecipeImageUri;

    public string ImageUriToThumbnail(string imageUri)
    {
        string[] parts = imageUri.Split("personalassistant");
        return parts[0] + "personalassistant/w_80,h_80,c_limit" + parts[1];
    }

    public async Task<string> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadAsync)}");

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

        metric.Finish();

        return _baseUrl + $"{uploadPath}/{uri}.{Format}";
    }

    public async Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadTempAsync)}");

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

        metric.Finish();

        return imageUri;
    }

    public async Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(UploadProfileTempAsync)}");

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

        metric.Finish();

        return _baseUrl + $"{uploadPath}/{uri}.{Format}";
    }

    public async Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (imageUriToCopy == _defaultRecipeImageUri)
        {
            return imageUriToCopy;
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CopyAndUploadAsync)}");

        var result = await _httpClient.GetAsync(new Uri(imageUriToCopy), cancellationToken);

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

        string imageUri = await UploadAsync(
            filePath: tempImagePath,
            uploadPath: uploadPath,
            template: template,
            metricsSpan: metric,
            cancellationToken
        );

        metric.Finish();

        return imageUri;
    }

    public async Task RemoveTempTagAsync(string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (IsDefaultImage(imageUri))
        {
            return;
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(RemoveTempTagAsync)}");

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

        metric.Finish();
    }

    public async Task DeleteAsync(string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        if (IsDefaultImage(imageUri))
        {
            return;
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}");

        string publicId = GetPublicIdFromUri(imageUri);

        DelResResult deleteResult = await _cloudinary.DeleteResourcesAsync(new DelResParams
        {
            PublicIds = new List<string> { publicId }
        }, cancellationToken);
        if (deleteResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}() returned error: {deleteResult.Error.Message}");
        }

        metric.Finish();
    }

    public async Task CreateFolderForUserAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}");

        CreateFolderResult result = await _cloudinary.CreateFolderAsync($"{_environment}/users/{userId}", cancellationToken);
        if (result.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}() returned error: {result.Error.Message}");
        }

        metric.Finish();
    }

    public async Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}");

        var publicIds = new List<string>();
        foreach (string uri in imageUris.Where(x => !IsDefaultImage(x)))
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

        metric.Finish();
    }

    public async Task DeleteTemporaryResourcesAsync(DateTime olderThan, CancellationToken cancellationToken)
    {
        var searchResult = _cloudinary.ListResourcesByTag("temp");
        if (!searchResult.Resources.Any())
        {
            return;
        }

        var publicIds = searchResult.Resources.Where(x => DateTime.Parse(x.CreatedAt) < olderThan).Select(x => x.PublicId).ToList();
        if (!publicIds.Any())
        {
            return;
        }

        DelResResult deleteResult = await _cloudinary.DeleteResourcesAsync(new DelResParams
        {
            PublicIds = publicIds
        }, cancellationToken);
        if (deleteResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteTemporaryResourcesAsync)}() returned error: {deleteResult.Error.Message}");
        }
    }

    private bool IsDefaultImage(string uri)
    {
        var defaults = new[] { _defaultProfileImageUri, _defaultRecipeImageUri };
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

    private string GetPublicIdFromUri(string uri)
    {
        var pathRegex = new Regex(@$"personalassistant\/(.*).{Format}");
        Match match = pathRegex.Match(uri);
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
