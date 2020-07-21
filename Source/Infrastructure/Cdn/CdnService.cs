using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Infrastructure.Cdn
{
    public class CdnService : ICdnService
    {
        private const string Format = "jpg";
        private readonly string _environment;
        private readonly string _defaultProfileImageUri;
        private readonly string _defaultRecipeImageUri;
        private readonly string _baseUrl;
        private readonly Dictionary<string, Transformation> _templates = new Dictionary<string, Transformation>
        {
            { "profile", new Transformation().Width(400).Height(400).Crop("lfill").FetchFormat(Format) },
            { "recipe", new Transformation().Width(640).Height(320).Crop("lfill").FetchFormat(Format) }
        };
        private readonly Dictionary<string, List<Transformation>> _eagerTransformTemplates = new Dictionary<string, List<Transformation>>
        {
            { "profile", new List<Transformation> { new Transformation().Named("profile_thumbnail") } },
            { "recipe", new List<Transformation>() }
        };
        private Cloudinary Cloudinary { get; set; }
        private readonly HttpClient _httpClient;

        public CdnService(
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
            Cloudinary = new Cloudinary(cloudinaryAccount);
            _httpClient = httpClient;
        }

        public string GetDefaultProfileImageUri() => _defaultProfileImageUri;

        public string GetDefaultRecipeUri() => _defaultRecipeImageUri;

        public string ImageUriToThumbnail(string imageUri)
        {
            string[] parts = imageUri.Split("personalassistant");
            return parts[0] + "personalassistant/t_profile_thumbnail" + parts[1];
        }

        public async Task<string> UploadAsync(string filePath, string uploadPath, string template)
        {
            string uri = GenerateRandomString();
            Transformation transformation = _templates[template];
            List<Transformation> eagerTransforms = _eagerTransformTemplates[template];

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_environment}/{uploadPath}/",
                PublicId = uri,
                Transformation = transformation,
                EagerTransforms = eagerTransforms
            };

            ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);

            File.Delete(filePath);

            return _baseUrl + $"{uploadPath}/{uri}.{Format}";
        }

        public async Task<string> UploadTempAsync(UploadTempImage model, IValidator<UploadTempImage> validator)
        {
            ValidateAndThrow(model, validator);

            string extension = Path.GetExtension(model.FileName);

            string tempImageName = Guid.NewGuid() + extension;
            string tempImagePath = Path.Combine(model.LocalTempPath, tempImageName);

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

            return imageUri;
        }

        public async Task<string> UploadProfileTempAsync(int userId, string filePath, string uploadPath, string template)
        {
            string uri = GenerateRandomString();
            Transformation transformation = _templates[template];
            List<Transformation> eagerTransforms = _eagerTransformTemplates[template];

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_environment}/{uploadPath}/",
                PublicId = uri,
                Transformation = transformation,
                EagerTransforms = eagerTransforms,
                Tags = "temp"
            };

            ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);

            File.Delete(filePath);

            return _baseUrl + $"{uploadPath}/{uri}.{Format}";
        }

        public async Task<string> CopyAndUploadAsync(string tempImagePath, string imageUriToCopy, string uploadPath, string template)
        {
            if (IsDefaultImage(imageUriToCopy))
            {
                return imageUriToCopy;
            }

            var result = await _httpClient.GetAsync(new Uri(imageUriToCopy));

            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            using (Stream image = await result.Content.ReadAsStreamAsync())
            {
                await image.CopyToAsync(stream);
            }

            string imageUri = await UploadAsync(
                filePath: tempImagePath,
                uploadPath: uploadPath,
                template: template
            );

            return imageUri;
        }

        public async Task RemoveTempTagAsync(string imageUri)
        {
            if (IsDefaultImage(imageUri))
            {
                return;
            }

            string publicId = GetPublicIdFromUri(imageUri);

            var tagParams = new TagParams
            {
                PublicIds = new List<string> { publicId },
                Tag = "temp",
                Command = TagCommand.Remove
            };

            TagResult tagResult = await Cloudinary.TagAsync(tagParams);
        }

        public async Task DeleteAsync(string imageUri)
        {
            if (IsDefaultImage(imageUri))
            {
                return;
            }

            string publicId = GetPublicIdFromUri(imageUri);

            DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = new List<string>() { publicId }
            });
        }

        public async Task DeleteUserResourcesAsync(int userId, IEnumerable<string> imageUris)
        {
            var publicIds = new List<string>();
            foreach (string uri in imageUris)
            {
                string publicId = GetPublicIdFromUri(uri);
                publicIds.Add(publicId);
            }

            DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = publicIds
            });

            DeleteFolderResult deleteFolderResult = await Cloudinary.DeleteFolderAsync($"{_environment}/users/{userId}");
        }

        public async Task DeleteTemporaryResourcesAsync(DateTime olderThan)
        {
            var searchResult = Cloudinary.ListResourcesByTag("temp");
            if (!searchResult.Resources.Any())
            {
                return;
            }

            var publicIds = searchResult.Resources.Where(x => DateTime.Parse(x.CreatedAt) < olderThan).Select(x => x.PublicId).ToList();

            DelResResult deleteResult = await Cloudinary.DeleteResourcesAsync(new DelResParams
            {
                PublicIds = publicIds
            });
        }

        private bool IsDefaultImage(string uri)
        {
            var defaults = new string[] { _defaultProfileImageUri, _defaultRecipeImageUri };
            return defaults.Contains(uri);
        }

        private async Task<string> UploadTempAsync(string filePath, string uploadPath, string template)
        {
            string uri = GenerateRandomString();
            Transformation transformation = _templates[template];
            List<Transformation> eagerTransforms = _eagerTransformTemplates[template];

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_environment}/{uploadPath}/",
                PublicId = uri,
                Transformation = transformation,
                EagerTransforms = eagerTransforms,
                Tags = "temp"
            };

            ImageUploadResult uploadResult = await Cloudinary.UploadAsync(uploadParams);

            File.Delete(filePath);

            return _baseUrl + $"{uploadPath}/{uri}.{Format}";
        }

        private string GetPublicIdFromUri(string uri)
        {
            var pathRegex = new Regex(@"users/.+");
            Match match = pathRegex.Match(uri);
            string path = match.Value;

            int periodIndex = path.LastIndexOf('.');
            path = path.Substring(0, periodIndex);

            return $"{_environment}/{path}";
        }

        private string GenerateRandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void ValidateAndThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
