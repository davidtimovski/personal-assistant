﻿using System.Text.RegularExpressions;
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
    private readonly Dictionary<string, Transformation> _templates = new()
    {
        { "profile", new Transformation().Width(400).Height(400).Crop("lfill").FetchFormat(Format) },
        { "recipe", new Transformation().Width(640).Height(320).Crop("lfill").FetchFormat(Format) }
    };
    private CloudinaryDotNet.Cloudinary Cloudinary { get; }
    private readonly HttpClient _httpClient;

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
        Cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryAccount);
        _httpClient = httpClient;
    }

    public string GetDefaultProfileImageUri() => _defaultProfileImageUri;

    public string GetDefaultRecipeImageUri() => _defaultRecipeImageUri;

    public string ImageUriToThumbnail(string imageUri)
    {
        string[] parts = imageUri.Split("personalassistant");
        return parts[0] + "personalassistant/w_80,h_80,c_limit" + parts[1];
    }

    public async Task<string> UploadAsync(string filePath, string uploadPath, string template, ISpan metricsSpan)
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

        ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(UploadAsync)}() returned error: {uploadResult.Error.Message}");
        }

        File.Delete(filePath);

        metric.Finish();

        return _baseUrl + $"{uploadPath}/{uri}.{Format}";
    }

    public async Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator, ISpan metricsSpan)
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
            await model.File.CopyToAsync(stream);
            await stream.FlushAsync();
        }

        string imageUri = await UploadTempAsync(
            filePath: tempImagePath,
            uploadPath: model.UploadPath,
            template: model.TransformationTemplate
        );

        metric.Finish();

        return imageUri;
    }

    public async Task<string> UploadProfileTempAsync(string filePath, string uploadPath, string template, ISpan metricsSpan)
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

        ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(UploadProfileTempAsync)}() returned error: {uploadResult.Error.Message}");
        }

        File.Delete(filePath);

        metric.Finish();

        return _baseUrl + $"{uploadPath}/{uri}.{Format}";
    }

    public async Task<string> CopyAndUploadAsync(string localTempPath, string imageUriToCopy, string uploadPath, string template, ISpan metricsSpan)
    {
        if (imageUriToCopy == _defaultRecipeImageUri)
        {
            return imageUriToCopy;
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CopyAndUploadAsync)}");

        var result = await _httpClient.GetAsync(new Uri(imageUriToCopy));

        if (!Directory.Exists(localTempPath))
        {
            Directory.CreateDirectory(localTempPath);
        }

        string tempImagePath = Path.Combine(localTempPath, Guid.NewGuid().ToString());

        using (var stream = new FileStream(tempImagePath, FileMode.Create))
        using (Stream image = await result.Content.ReadAsStreamAsync())
        {
            await image.CopyToAsync(stream);
        }

        string imageUri = await UploadAsync(
            filePath: tempImagePath,
            uploadPath: uploadPath,
            template: template,
            metricsSpan: metric
        );

        metric.Finish();

        return imageUri;
    }

    public async Task RemoveTempTagAsync(string imageUri, ISpan metricsSpan)
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

        TagResult tagResult = await Cloudinary.TagAsync(tagParams);
        if (tagResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(RemoveTempTagAsync)}() returned error: {tagResult.Error.Message}");
        }

        metric.Finish();
    }

    public async Task DeleteAsync(string imageUri, ISpan metricsSpan)
    {
        if (IsDefaultImage(imageUri))
        {
            return;
        }

        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}");

        string publicId = GetPublicIdFromUri(imageUri);

        DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
        {
            PublicIds = new List<string> { publicId }
        });
        if (deleteResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteAsync)}() returned error: {deleteResult.Error.Message}");
        }

        metric.Finish();
    }

    public async Task CreateFolderForUserAsync(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}");

        CreateFolderResult result = await Cloudinary.CreateFolderAsync($"{_environment}/users/{userId}");
        if (result.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(CreateFolderForUserAsync)}() returned error: {result.Error.Message}");
        }

        metric.Finish();
    }

    public async Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}");

        var publicIds = new List<string>();
        foreach (string uri in imageUris.Where(x => !IsDefaultImage(x)))
        {
            string publicId = GetPublicIdFromUri(uri);
            publicIds.Add(publicId);
        }

        DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
        {
            PublicIds = publicIds
        });
        if (deleteResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}() returned error: {deleteResult.Error.Message}");
        }

        DeleteFolderResult deleteFolderResult = await Cloudinary.DeleteFolderAsync($"{_environment}/users/{userId}");
        if (deleteFolderResult.Error != null)
        {
            throw new Exception($"{nameof(CloudinaryService)}.{nameof(DeleteUserResourcesAsync)}() returned error: {deleteFolderResult.Error.Message}");
        }

        metric.Finish();
    }

    public async Task DeleteTemporaryResourcesAsync(DateTime olderThan)
    {
        var searchResult = Cloudinary.ListResourcesByTag("temp");
        if (!searchResult.Resources.Any())
        {
            return;
        }

        var publicIds = searchResult.Resources.Where(x => DateTime.Parse(x.CreatedAt) < olderThan).Select(x => x.PublicId).ToList();
        if (!publicIds.Any())
        {
            return;
        }

        DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
        {
            PublicIds = publicIds
        });
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

    private async Task<string> UploadTempAsync(string filePath, string uploadPath, string template)
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

        ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);
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