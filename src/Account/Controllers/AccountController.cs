using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Account.Models;
using Account.ViewModels.Account;
using Account.ViewModels.Home;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Common;
using Application.Contracts.CookingAssistant.Recipes;
using Application.Contracts.ToDoAssistant.Lists;
using Auth0.AspNetCore.Authentication;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Account.Controllers;

[Authorize]
public class AccountController : Controller
{
    //private readonly IEmailTemplateService _emailTemplateService;
    private readonly IAccountService _accountService;
    private readonly IListService _listService;
    private readonly IRecipeService _recipeService;
    private readonly ICdnService _cdnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<AccountController> _localizer;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        //IEmailTemplateService emailTemplateService,
        IAccountService accountService,
        IListService listService,
        IRecipeService recipeService,
        ICdnService cdnService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IStringLocalizer<AccountController> localizer,
        IWebHostEnvironment webHostEnvironment,
        ILogger<AccountController> logger)
    {
        //_emailTemplateService = emailTemplateService;
        _accountService = accountService;
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
    public async Task Login(string returnUrl)
    {
        if (returnUrl == null)
        {
            returnUrl = _configuration["Urls:PersonalAssistant"];
        }

        // Indicate here where Auth0 should redirect the user after a login.
        // Note that the resulting absolute Uri must be added to the
        // **Allowed Callback URLs** settings for the app.
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder().WithRedirectUri(returnUrl).Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task Logout()
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
           // Indicate here where Auth0 should redirect the user after a logout.
           // Note that the resulting absolute Uri must be added to the
           // **Allowed Logout URLs** settings for the app.
           .WithRedirectUri(_configuration["Urls:PersonalAssistant"])
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

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl)
    {
        return View(model);
        //if (!ModelState.IsValid)
        //{
        //    return View(model);
        //}
            
        //ViewData["ReturnUrl"] = returnUrl;
            
        //var user = new ApplicationUser
        //{
        //    Name = model.Name.Trim(),
        //    UserName = model.Email.Trim(),
        //    Email = model.Email.Trim(),
        //    Language = model.Language,
        //    ToDoNotificationsEnabled = false,
        //    CookingNotificationsEnabled = false,
        //    ImperialSystem = false,
        //    ImageUri = _cdnService.GetDefaultProfileImageUri(),
        //    DateRegistered = DateTime.UtcNow
        //};

        //IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        //if (!result.Succeeded)
        //{
        //    AddIdentityErrors(result, nameof(Register));
        //    return View(model);
        //}

        //string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //var callbackUrl = Url.Action("confirm-email", "Account", new { userId = user.Id, token, returnUrl }, HttpContext.Request.Scheme);
        //await _emailTemplateService.EnqueueRegisterConfirmationEmailAsync(user.Name, user.Email, new Uri(callbackUrl), model.Language);

        //_ = _emailTemplateService.EnqueueNewRegistrationEmailAsync(user.Name, user.Email);

        //SetLanguageCookie(model.Language);

        //return RedirectToAction(nameof(Login), new { alert = GenerateLoginAlertFromRegistrationEmail(user.Email) });
    }

    //[HttpGet]
    //[AllowAnonymous]
    //[ActionName("confirm-email")]
    //public async Task<IActionResult> ConfirmEmail(int userId, string token, string returnUrl)
    //{
    //    ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
    //    if (user == null)
    //    {
    //        return RedirectToAction(nameof(Login));
    //    }

    //    if (user.EmailConfirmed)
    //    {
    //        return RedirectToAction(nameof(Login));
    //    }

    //    IdentityResult confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);
    //    if (!confirmEmailResult.Succeeded)
    //    {
    //        return RedirectToAction(nameof(Login));
    //    }

    //    await CreateRequiredDataAsync(user.Id);
    //    await CreateSamplesAsync(user.Id);

    //    _ = _emailTemplateService.EnqueueNewEmailVerificationEmailAsync(user.Name, user.Email);

    //    return RedirectToAction(nameof(Login), new { returnUrl, alert = LoginAlert.RegistrationConfirmed });
    //}

    [HttpGet]
    [ActionName("change-password")]
    public async Task<IActionResult> ChangePassword()
    {
        using HttpClient httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0ClientSecret"]);
        await Auth0ManagementUtil.InitializeAsync(httpClient, config);

        var profile = await Auth0ManagementUtil.GetUserProfileAsync(httpClient, "auth0|634703e9a061a272e193a11d");

        var viewModel = new ChangePasswordViewModel
        {
            Email = profile.email
        };

        return View(viewModel);
    }

    [HttpPost]
    [ActionName("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0ClientSecret"]);
        await Auth0ManagementUtil.InitializeAsync(httpClient, config);

        try
        {
            await Auth0ManagementUtil.ResetPasswordAsync(httpClient, _configuration["Auth0:ClientId"], model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resetting user password failed");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
            return View(model);
        }

        return RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.PasswordResetEmailSent });
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
        var response = JsonConvert.DeserializeObject<ReCaptchaResponse>(await result.Content.ReadAsStringAsync());

        return Ok(response.Score);
    }

    //[HttpGet]
    //public IActionResult Delete()
    //{
    //    return View();
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Delete(DeleteViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return View(model);
    //    }
            
    //    ApplicationUser user = await _userManager.GetUserAsync(User);

    //    var passwordIsValid = await _userManager.CheckPasswordAsync(user, model.Password);
    //    if (passwordIsValid)
    //    {
    //        await _signInManager.SignOutAsync();

    //        IdentityResult result = await _userManager.DeleteAsync(user);
    //        if (result.Succeeded)
    //        {
    //            var filePaths = new List<string>();
    //            if (user.ImageUri != null)
    //            {
    //                filePaths.Add($"users/{user.Id}/{user.ImageUri}");
    //            }

    //            var recipeUris = _recipeService.GetAllImageUris(user.Id);
    //            if (recipeUris.Any())
    //            {
    //                filePaths.AddRange(recipeUris.Select(uri => $"users/{user.Id}/recipes/{uri}"));

    //                await _cdnService.DeleteUserResourcesAsync(user.Id, filePaths);
    //            }

    //            return RedirectToAction(nameof(Login), new { alert = LoginAlert.AccountDeleted });
    //        }

    //        ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
    //    }
    //    else
    //    {
    //        ModelState.AddModelError(string.Empty, _localizer["IncorrectPassword"]);
    //    }

    //    return View(model);
    //}

    [HttpGet]
    [ActionName("edit-profile")]
    public async Task<IActionResult> EditProfile()
    {
        using HttpClient httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0ClientSecret"]);
        await Auth0ManagementUtil.InitializeAsync(httpClient, config);

        var profile = await Auth0ManagementUtil.GetUserProfileAsync(httpClient, "auth0|634703e9a061a272e193a11d");

        var viewModel = new ViewProfileViewModel
        {
            Name = profile.name,
            Language = profile.user_metadata.Language,
            ImageUri = profile.user_metadata.ImageUri,
            DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
            BaseUrl = _configuration["Urls:PersonalAssistant"]
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
                ImageUri = model.ImageUri,
                DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
                BaseUrl = _configuration["Urls:PersonalAssistant"]
            });
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        var config = new Auth0ManagementUtilConfig(_configuration["Auth0:Domain"], _configuration["Auth0:ClientId"], _configuration["Auth0ClientSecret"]);
        await Auth0ManagementUtil.InitializeAsync(httpClient, config);

        var profile = await Auth0ManagementUtil.GetUserProfileAsync(httpClient, "auth0|634703e9a061a272e193a11d");

        string userCurrentLanguage = profile.user_metadata.Language;
        profile.name = model.Name.Trim();
        profile.user_metadata.Language = model.Language;
        string oldImageUri = profile.user_metadata.ImageUri;
        profile.user_metadata.ImageUri = string.IsNullOrEmpty(model.ImageUri) ? null : model.ImageUri;

        try
        {
            await Auth0ManagementUtil.UpdateUserProfileAsync(httpClient, "auth0|634703e9a061a272e193a11d", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Updating user profile failed");

            ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);

            return View(new ViewProfileViewModel
            {
                Name = model.Name,
                Language = model.Language,
                ImageUri = model.ImageUri,
                DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
                BaseUrl = _configuration["Urls:PersonalAssistant"]
            });
        }

        // If the user changed his image
        if (oldImageUri != model.ImageUri)
        {
            // and had a previous one, delete it
            if (oldImageUri != null)
            {
                await _cdnService.DeleteAsync(oldImageUri);
            }

            // and has a new one, remove its temp tag
            if (profile.user_metadata.ImageUri != null)
            {
                await _cdnService.RemoveTempTagAsync(profile.user_metadata.ImageUri);
            }
        }

        SetLanguageCookie(model.Language);

        if (model.Language != userCurrentLanguage)
        {
            await Logout();
            return RedirectToAction(nameof(Login));
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

        if (!new [] { ".JPG", ".PNG", ".JPEG" }.Contains(extension.ToUpperInvariant()))
        {
            ModelState.AddModelError(string.Empty, _localizer["InvalidImageFormat"]);
            return new UnprocessableEntityObjectResult(ModelState);
        }

        string tempImageName = Guid.NewGuid() + extension;
        string tempImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp", tempImageName);

        if (image.Length > 0)
        {
            try
            {
                using (var stream = new FileStream(tempImagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                //ApplicationUser user = await _userManager.GetUserAsync(User);

                // TODO: get user id here

                string imageUri = await _cdnService.UploadProfileTempAsync(
                    filePath: tempImagePath,
                    uploadPath: $"users/2",
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

    private async Task CreateRequiredDataAsync(int userId)
    {
        var createMainAccount = new CreateMainAccount
        {
            UserId = userId,
            Name = _localizer["MainAccountName"]
        };
        await _accountService.CreateMainAsync(createMainAccount);
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

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index));
    }

    #endregion
}
