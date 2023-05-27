using System.Text.Json;
using Account.Web.Models;
using Account.Web.Services;
using Account.Web.ViewModels.Account;
using Account.Web.ViewModels.Home;
using Api.Common;
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
using Microsoft.FSharp.Core;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using static Accountant.Persistence.Fs.AccountsRepository;

namespace Account.Web.Controllers;

[Authorize]
public class AccountController : BaseController
{
    private readonly IUsersRepository _usersRepository;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserService _userService;
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
        var tr = Metrics.StartTransaction(
            $"{Request.Method} account/reset-password",
            $"{nameof(AccountController)}.{nameof(ResetPassword)}"
        );

        var viewModel = new ResetPasswordViewModel();

        if (User?.Identity.IsAuthenticated == true)
        {
            using var httpClient = await InitializeAuth0ClientAsync(tr);

            var user = await Auth0Proxy.GetUserAsync(httpClient, AuthId, tr);
            viewModel.Email = user.email;
        }

        tr.Finish();

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

        var tr = Metrics.StartTransaction(
            $"{Request.Method} account/reset-password",
            $"{nameof(AccountController)}.{nameof(ResetPassword)}"
        );

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync(tr);

            await Auth0Proxy.ResetPasswordAsync(httpClient, _configuration["Auth0:ClientId"], model.Email, tr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Resetting user password failed for user: {model.Email}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View(model);
        }
        finally
        {
            tr.Finish();
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

        var tr = Metrics.StartTransaction(
            $"{Request.Method} account/register",
            $"{nameof(AccountController)}.{nameof(Register)}"
        );

        ViewData["ReturnUrl"] = returnUrl;

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync(tr);

            var authId = await Auth0Proxy.RegisterUserAsync(httpClient, model.Email, model.Password, model.Name, tr);
            var userId = await _userService.CreateAsync(authId, model.Email, model.Name, model.Language, model.Culture, _cdnService.GetDefaultProfileImageUri(), tr);

            await _cdnService.CreateFolderForUserAsync(userId, tr);

            await CreateRequiredDataAsync(userId, tr);
            await CreateSamplesAsync(userId, tr);
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
        finally
        {
            tr.Finish();
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
        var tr = Metrics.StartTransaction(
            $"{Request.Method} account/verify-recaptcha",
            $"{nameof(AccountController)}.{nameof(VerifyReCaptcha)}"
        );

        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", _configuration["Account:ReCaptchaSecret"]),
            new KeyValuePair<string, string>("response", model.Token)
        });

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        using var result = await httpClient.PostAsync(new Uri(_configuration["ReCaptchaVerificationUrl"]), content);
        var response = JsonSerializer.Deserialize<ReCaptchaResponse>(await result.Content.ReadAsStringAsync());

        tr.Finish();

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
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} /account/delete",
            $"{nameof(AccountController)}.{nameof(DeleteAccount)}",
            UserId
        );

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync(tr);

            await Auth0Proxy.DeleteUserAsync(httpClient, AuthId, tr);

            // Delete resources
            var user = _userService.Get(UserId);
            var filePaths = new List<string> { user.ImageUri };
            IEnumerable<string> recipeUris = _recipeService.GetAllImageUris(user.Id);
            filePaths.AddRange(recipeUris);
            await _cdnService.DeleteUserResourcesAsync(user.Id, filePaths, tr);

            await _usersRepository.DeleteAsync(UserId, tr);

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
        finally
        {
            tr.Finish();
        }

        return RedirectToAction(nameof(HomeController.Index), "Home", new { alert = IndexAlert.AccountDeleted });
    }

    [HttpGet]
    [ActionName("edit-profile")]
    public async Task<IActionResult> EditProfile()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} account/edit-profile",
            $"{nameof(AccountController)}.{nameof(EditProfile)}",
            UserId
        );

        using var httpClient = await InitializeAuth0ClientAsync(tr);

        var authUser = await Auth0Proxy.GetUserAsync(httpClient, AuthId, tr);
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

        tr.Finish();

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

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} account/edit-profile",
            $"{nameof(AccountController)}.{nameof(EditProfile)}",
            UserId
        );

        try
        {
            using var httpClient = await InitializeAuth0ClientAsync(tr);

            await Auth0Proxy.UpdateNameAsync(httpClient, AuthId, model.Name, tr);

            var imageUri = string.IsNullOrEmpty(model.ImageUri) ? null : model.ImageUri;
            await _userService.UpdateProfileAsync(UserId, model.Name, model.Language, model.Culture, imageUri, tr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Updating user profile failed for user: {model.Name}");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);

            tr.Finish();

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
                await _cdnService.DeleteAsync(oldImageUri, tr);
            }

            // and has a new one, remove its temp tag
            if (user.ImageUri != null)
            {
                await _cdnService.RemoveTempTagAsync(user.ImageUri, tr);
            }
        }

        SetLanguageCookie(model.Language);

        tr.Finish();

        return model.Language != user.Language
            ? RedirectToAction(nameof(Logout), "Account", new { returnUrlSlug = "?alert=" + IndexAlert.LanguageChanged })
            : RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.ProfileUpdated });
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

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} account/upload-profile-image",
            $"{nameof(AccountController)}.{nameof(UploadProfileImage)}",
            UserId
        );

        if (image.Length == 0)
        {
            ModelState.AddModelError(string.Empty, _localizer["InvalidImage"]);
            return new UnprocessableEntityObjectResult(ModelState);
        }

        string tempFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp");
        if (!Directory.Exists(tempFolder))
        {
            Directory.CreateDirectory(tempFolder);
        }

        try
        {
            string tempImagePath = Path.Combine(tempFolder, Guid.NewGuid() + extension);
            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            string imageUri = await _cdnService.UploadProfileTempAsync(
                filePath: tempImagePath,
                uploadPath: $"users/{UserId}",
                template: "profile",
                tr
            );

            return StatusCode(201, new { imageUri });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(UploadProfileImage));
            throw;
        }
        finally
        {
            tr.Finish();
        }
    }

    private async Task<HttpClient> InitializeAuth0ClientAsync(ITransaction tr)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0:ClientSecret"]);
        await Auth0Proxy.InitializeAsync(httpClient, config, tr);

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

    // TODO: Breaking microservice design. Implement with message queue or HTTP call.
    private async Task CreateRequiredDataAsync(int userId, ISpan metricsSpan)
    {
        var now = DateTime.UtcNow;
        await createMain(new Accountant.Persistence.Fs.Models.Account(0, userId, _localizer["MainAccountName"], true, "EUR", FSharpOption<decimal>.None, now, now), _configuration["ConnectionString"], metricsSpan);
    }

    private async Task CreateSamplesAsync(int userId, ISpan metricsSpan)
    {
        var sampleListTranslations = new Dictionary<string, string>
        {
            { "SampleListName", _localizer["SampleListName"] },
            { "SampleListTask1", _localizer["SampleListTask1"] },
            { "SampleListTask2", _localizer["SampleListTask2"] },
            { "SampleListTask3", _localizer["SampleListTask3"] }
        };
        await _listService.CreateSampleAsync(userId, sampleListTranslations, metricsSpan);

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
