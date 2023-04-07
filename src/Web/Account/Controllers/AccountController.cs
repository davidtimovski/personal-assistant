using System.Text.Json;
using Account.Models;
using Account.Services;
using Account.ViewModels.Account;
using Account.ViewModels.Home;
using Accountant.Application.Fs.Services;
using Accountant.Persistence.Fs;
using Auth0.AspNetCore.Authentication;
using CookingAssistant.Application.Contracts.Recipes;
using Core.Application.Contracts;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ToDoAssistant.Application.Contracts.Lists;
using static Accountant.Persistence.Fs.CommonRepository;

namespace Account.Controllers;

[Authorize]
public class AccountController : BaseController
{
    private readonly IUsersRepository _usersRepository;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserService _userService;
    private readonly AccountantContext _accountantContext;
    private readonly IListService _listService;
    private readonly IRecipeService _recipeService;
    private readonly ICdnService _cdnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<AccountController> _localizer;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IEmailTemplateService emailTemplateService,
        IUserService userService,
        AccountantContext accountantContext,
        IListService listService,
        IRecipeService recipeService,
        ICdnService cdnService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IStringLocalizer<AccountController> localizer,
        IWebHostEnvironment webHostEnvironment,
        ILogger<AccountController> logger) : base(userIdLookup, usersRepository)
    {
        _usersRepository = usersRepository;
        _emailTemplateService = emailTemplateService;
        _userService = userService;
        _accountantContext = accountantContext;
        _listService = listService;
        _recipeService = recipeService;
        _cdnService = cdnService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _localizer = localizer;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task Login()
    {
        // Indicate here where Auth0 should redirect the user after a login.
        // Note that the resulting absolute Uri must be added to the
        // **Allowed Callback URLs** settings for the app.
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(_configuration["Urls:Account"])
            .Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }

    [HttpGet]
    [AllowAnonymous]
    [ActionName("reset-password")]
    public async Task<IActionResult> ResetPassword()
    {
        var viewModel = new ResetPasswordViewModel();

        if (User?.Identity.IsAuthenticated == true)
        {
            using var httpClient = await InitializeAuth0ClientAsync();

            var user = await Auth0Proxy.GetUserAsync(httpClient, AuthId);
            viewModel.Email = user.email;
        }

        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ActionName("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync();

            await Auth0Proxy.ResetPasswordAsync(httpClient, _configuration["Auth0:ClientId"], model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Resetting user password failed for user: {model.Email}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View(model);
        }

        return RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.PasswordResetEmailSent });
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpGet]
    public async Task Logout(string returnUrlSlug)
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
           // Indicate here where Auth0 should redirect the user after a logout.
           // Note that the resulting absolute Uri must be added to the
           // **Allowed Logout URLs** settings for the app.
           .WithRedirectUri(_configuration["Urls:Account"] + returnUrlSlug)
           .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl)
    {
        if (User?.Identity.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Overview), "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ViewData["ReturnUrl"] = returnUrl;

        try
        {
            //using var httpClient = await InitializeAuth0ClientAsync();

            //var authId = await Auth0Proxy.RegisterUserAsync(httpClient, model.Email, model.Password, model.Name);
            var userId = await _userService.CreateAsync("adawdawdada", model.Email, model.Name, model.Language, model.Culture, _cdnService.GetDefaultProfileImageUri());

            //await _cdnService.CreateFolderForUserAsync(userId);

            await CreateRequiredDataAsync(userId);
            // await CreateSamplesAsync(userId);
        }
        catch (PasswordTooWeakException)
        {
            ModelState.AddModelError(nameof(model.Password), _localizer["PasswordTooWeak"]);
            return View(model);
        }
        catch (Auth0Exception ex)
        {
            _logger.LogError(ex, $"User registration failed for: {model.Email}. Auth0 response: {ex.Message}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"User registration failed for: {model.Email}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View(model);
        }

        _ = _emailTemplateService.EnqueueNewRegistrationEmailAsync(model.Name.Trim(), model.Email.Trim());

        SetLanguageCookie(model.Language);

        return RedirectToAction(nameof(HomeController.Index), "Home", new { alert = IndexAlert.SuccessfullyRegistered });
    }

    [HttpPost]
    [AllowAnonymous]
    [ActionName("verify-recaptcha")]
    public async Task<IActionResult> VerifyReCaptcha(VerifyReCaptchaViewModel model)
    {
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", _configuration["ReCaptchaSecret"]),
            new KeyValuePair<string, string>("response", model.Token)
        });

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        var result = await httpClient.PostAsync(new Uri(_configuration["ReCaptchaVerificationUrl"]), content);
        var response = JsonSerializer.Deserialize<ReCaptchaResponse>(await result.Content.ReadAsStringAsync());

        return Ok(response.Score);
    }

    [HttpGet]
    public IActionResult Delete()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            using var httpClient = await InitializeAuth0ClientAsync();

            await Auth0Proxy.DeleteUserAsync(httpClient, AuthId);

            // Delete resources
            var user = _userService.Get(UserId);
            var filePaths = new List<string> { user.ImageUri };
            IEnumerable<string> recipeUris = _recipeService.GetAllImageUris(user.Id);
            filePaths.AddRange(recipeUris);
            await _cdnService.DeleteUserResourcesAsync(user.Id, filePaths);

            await _usersRepository.DeleteAsync(UserId);

            // Logout
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder().Build();
            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"User account deletion failed for user: {UserId}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View("Delete");
        }

        return RedirectToAction(nameof(HomeController.Index), "Home", new { alert = IndexAlert.AccountDeleted });
    }

    [HttpGet]
    [ActionName("edit-profile")]
    public async Task<IActionResult> EditProfile()
    {
        using var httpClient = await InitializeAuth0ClientAsync();

        var authUser = await Auth0Proxy.GetUserAsync(httpClient, AuthId);
        var user = _userService.Get(UserId);

        var viewModel = new ViewProfileViewModel
        {
            Name = authUser.name,
            Language = user.Language,
            Culture = user.Culture,
            ImageUri = user.ImageUri,
            DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
            BaseUrl = _configuration["Urls:Account"]
        };

        return View(viewModel);
    }

    [HttpPost]
    [ActionName("edit-profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(new ViewProfileViewModel
            {
                Name = model.Name,
                Language = model.Language,
                Culture = model.Culture,
                ImageUri = model.ImageUri,
                DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
                BaseUrl = _configuration["Urls:Account"]
            });
        }

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync();

            await Auth0Proxy.UpdateNameAsync(httpClient, AuthId, model.Name);

            var imageUri = string.IsNullOrEmpty(model.ImageUri) ? null : model.ImageUri;
            await _userService.UpdateProfileAsync(UserId, model.Name, model.Language, model.Culture, imageUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Updating user profile failed for user: {model.Name}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);

            return View(new ViewProfileViewModel
            {
                Name = model.Name,
                Language = model.Language,
                Culture = model.Culture,
                ImageUri = model.ImageUri,
                DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
                BaseUrl = _configuration["Urls:Account"]
            });
        }

        var user = _userService.Get(UserId);

        string oldImageUri = user.ImageUri;

        // If the user changed his image
        if (oldImageUri != model.ImageUri)
        {
            // and had a previous one, delete it
            if (oldImageUri != null)
            {
                await _cdnService.DeleteAsync(oldImageUri);
            }

            // and has a new one, remove its temp tag
            if (user.ImageUri != null)
            {
                await _cdnService.RemoveTempTagAsync(user.ImageUri);
            }
        }

        SetLanguageCookie(model.Language);

        if (model.Language != user.Language)
        {
            return RedirectToAction(nameof(Logout), "Account", new { returnUrlSlug = "?alert=" + IndexAlert.LanguageChanged });
        }

        return RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.ProfileUpdated });
    }

    [HttpPost]
    [ActionName("upload-profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile image)
    {
        if (image.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError(string.Empty, _localizer["ImageTooLarge", 10]);
            return new UnprocessableEntityObjectResult(ModelState);
        }

        string extension = Path.GetExtension(image.FileName);

        if (!new[] { ".JPG", ".PNG", ".JPEG" }.Contains(extension.ToUpperInvariant()))
        {
            ModelState.AddModelError(string.Empty, _localizer["InvalidImageFormat"]);
            return new UnprocessableEntityObjectResult(ModelState);
        }

        if (image.Length > 0)
        {
            try
            {
                string tempFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp");
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                string tempImagePath = Path.Combine(tempFolder, Guid.NewGuid() + extension);
                using (var stream = new FileStream(tempImagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                string imageUri = await _cdnService.UploadProfileTempAsync(
                    filePath: tempImagePath,
                    uploadPath: $"users/{UserId}",
                    template: "profile"
                );

                return StatusCode(201, new { imageUri });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(UploadProfileImage));
                throw;
            }
        }

        ModelState.AddModelError(string.Empty, _localizer["InvalidImage"]);
        return new UnprocessableEntityObjectResult(ModelState);
    }

    private async Task<HttpClient> InitializeAuth0ClientAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0:ClientSecret"]);
        await Auth0Proxy.InitializeAsync(httpClient, config);

        return httpClient;
    }

    #region Helpers

    private void SetLanguageCookie(string language)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                HttpOnly = false,
                SameSite = SameSiteMode.Strict
            }
        );
    }

    // TODO: Breaking microservice design
    private async Task CreateRequiredDataAsync(int userId)
    {
        var mainAccount = AccountService.prepareForCreateMain(userId, _localizer["MainAccountName"]);
        await AccountsRepository.create(mainAccount, _accountantContext);
    }

    private async Task CreateSamplesAsync(int userId)
    {
        var sampleListTranslations = new Dictionary<string, string>
        {
            { "SampleListName", _localizer["SampleListName"] },
            { "SampleListTask1", _localizer["SampleListTask1"] },
            { "SampleListTask2", _localizer["SampleListTask2"] },
            { "SampleListTask3", _localizer["SampleListTask3"] }
        };
        await _listService.CreateSampleAsync(userId, sampleListTranslations);

        var sampleRecipeTranslations = new Dictionary<string, string>
        {
            { "SampleRecipeName", _localizer["SampleRecipeName"] },
            { "SampleRecipeDescription", _localizer["SampleRecipeDescription"] },
            { "SampleRecipeInstructions", _localizer["SampleRecipeInstructions"] }
        };
        await _recipeService.CreateSampleAsync(userId, sampleRecipeTranslations);
    }

    #endregion
}
