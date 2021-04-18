using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Auth.Models;
using Auth.Services;
using Auth.ViewModels.Account;
using Auth.ViewModels.Home;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Infrastructure.Identity;

namespace Auth.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly IEmailTemplateService _emailTemplateService;
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
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            IEmailTemplateService emailTemplateService,
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
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _emailTemplateService = emailTemplateService;
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
        public IActionResult Login(string returnUrl, LoginAlert alert)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(HomeController.Overview));
            }

            var language = CultureInfo.CurrentCulture.Name;

            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                Alert = alert
            };

            if (returnUrl != null)
            {
                if (returnUrl.Contains("to-do-assistant", StringComparison.OrdinalIgnoreCase))
                {
                    viewModel.ButtonLabel = LoginButtonLabel.ToDoAssistant;
                }
                else if (returnUrl.Contains("cooking-assistant", StringComparison.OrdinalIgnoreCase))
                {
                    viewModel.ButtonLabel = LoginButtonLabel.CookingAssistant;
                }
                else if (returnUrl.Contains("accountant", StringComparison.OrdinalIgnoreCase))
                {
                    viewModel.ButtonLabel = LoginButtonLabel.Accountant;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if we are in the context of an authorization request
                AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    // Set language cookie
                    ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);

                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                    Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(user.Language)),
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            IsEssential = true,
                            Path = "/",
                            HttpOnly = false,
                            SameSite = SameSiteMode.Strict
                        }
                    );

                    if (model.ReturnUrl != null)
                    {
                        return RedirectToLocal(model.ReturnUrl);
                    }
                    return RedirectToAction(nameof(HomeController.Overview), "Home");
                }
                if (result.IsNotAllowed)
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "Email confirmation required", clientId: context?.Client.ClientId));

                    return RedirectToAction(nameof(AccountController.Login), new { model.ReturnUrl, alert = LoginAlert.EmailConfirmationRequired });
                }
                if (result.IsLockedOut)
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "Locked out", clientId: context?.Client.ClientId));

                    ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
                    var lockoutEnd = (DateTimeOffset)await _userManager.GetLockoutEndDateAsync(user);
                    var lockoutViewModel = new LockoutViewModel
                    {
                        LockoutEndMinutes = (int)(lockoutEnd.UtcDateTime - DateTime.UtcNow).TotalMinutes + 1
                    };
                    return View("Lockout", lockoutViewModel);
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "Invalid credentials", clientId: context?.Client.ClientId));

                    ModelState.AddModelError(string.Empty, _localizer["InvalidLoginAttempt"]);
                    return View(nameof(AccountController.Login), model);
                }
            }

            return View(nameof(AccountController.Login), model);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("logged-out", vm);
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
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Name = model.Name.Trim(),
                    UserName = model.Email.Trim(),
                    Email = model.Email.Trim(),
                    Language = model.Language,
                    ToDoNotificationsEnabled = false,
                    CookingNotificationsEnabled = false,
                    ImperialSystem = false,
                    ImageUri = _cdnService.GetDefaultProfileImageUri(),
                    DateRegistered = DateTime.UtcNow
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("confirm-email", "Account", new { userId = user.Id, token, returnUrl }, HttpContext.Request.Scheme);
                    await _emailTemplateService.EnqueueRegisterConfirmationEmailAsync(user.Name, user.Email, new Uri(callbackUrl), model.Language);

                    // Notify admin
                    _ = _emailTemplateService.EnqueueNewRegistrationEmailAsync(user.Name, user.Email);

                    SetLanguageCookie(model.Language);

                    return RedirectToAction(nameof(AccountController.Login), new { alert = GenerateLoginAlertFromRegistrationEmail(user.Email) });
                }

                AddIdentityErrors(result, nameof(AccountController.Register));
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(int userId, string token, string returnUrl)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login));
            }

            IdentityResult confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!confirmEmailResult.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.Login));
            }

            await CreateRequiredDataAsync(user.Id);
            await CreateSamplesAsync(user.Id);

            // Notify admin
            _ = _emailTemplateService.EnqueueNewEmailVerificationEmailAsync(user.Name, user.Email);

            return RedirectToAction(nameof(AccountController.Login), new { returnUrl, alert = LoginAlert.RegistrationConfirmed });
        }

        [HttpGet]
        [ActionName("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ActionName("change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);

                IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, _localizer["IncorrectCurrentPassword"]);
                    return View(model);
                }

                return RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.PasswordChanged });
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("forgot-password")]
        public IActionResult ForgotPassword()
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(HomeController.Overview), "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(AccountController.Login), new { alert = LoginAlert.PasswordResetEmailSent });
                }

                bool emailIsConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                bool userHasPassword = await _userManager.HasPasswordAsync(user);

                if (emailIsConfirmed && userHasPassword)
                {
                    string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    string callbackUrl = Url.Action("reset-password", "Account", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);
                    string language = CultureInfo.CurrentCulture.Name;

                    await _emailTemplateService.EnqueuePasswordResetEmailAsync(user.Email, new Uri(callbackUrl), language);
                    return RedirectToAction(nameof(AccountController.Login), new { alert = GenerateLoginAlertFromPasswordResetEmail(user.Email) });
                }
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("reset-password")]
        public IActionResult ResetPassword(int userId, string token)
        {
            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction(nameof(AccountController.Login), new { alert = LoginAlert.PasswordReset });
                }

                IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(AccountController.Login), new { alert = LoginAlert.PasswordReset });
                }

                AddIdentityErrors(result, nameof(AccountController.ResetPassword));
            }

            return View(model);
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

        [HttpGet]
        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(DeleteViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);

                var passwordIsValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (passwordIsValid)
                {
                    await _signInManager.SignOutAsync();

                    IdentityResult result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        var filePaths = new List<string>();
                        if (user.ImageUri != null)
                        {
                            filePaths.Add($"users/{user.Id}/{user.ImageUri}");
                        }

                        var recipeUris = await _recipeService.GetAllImageUrisAsync(user.Id);
                        if (recipeUris.Any())
                        {
                            filePaths.AddRange(recipeUris.Select(uri => $"users/{user.Id}/recipes/{uri}"));

                            await _cdnService.DeleteUserResourcesAsync(user.Id, filePaths);
                        }

                        return RedirectToAction(nameof(AccountController.Login), new { alert = LoginAlert.AccountDeleted });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["IncorrectPassword"]);
                }
            }

            return View(model);
        }

        [HttpGet]
        [ActionName("edit-profile")]
        public async Task<IActionResult> EditProfile()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            var viewProfileViewModel = new ViewProfileViewModel
            {
                Name = user.Name,
                Language = user.Language,
                ImageUri = user.ImageUri,
                DefaultImageUri = _cdnService.GetDefaultProfileImageUri(),
                BaseUrl = _configuration["Urls:PersonalAssistant"]
            };

            return View(viewProfileViewModel);
        }

        [HttpPost]
        [ActionName("edit-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                string userCurrentLanguage = user.Language;
                user.Name = model.Name.Trim();
                user.Language = model.Language;
                string oldImageUri = user.ImageUri;
                user.ImageUri = string.IsNullOrEmpty(model.ImageUri) ? null : model.ImageUri;

                IdentityResult updateProfileResult = await _userManager.UpdateAsync(user);
                if (!updateProfileResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, _localizer["AnErrorOccurred"]);
                    return View(model);
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
                    if (user.ImageUri != null)
                    {
                        await _cdnService.RemoveTempTagAsync(user.ImageUri);
                    }
                }

                SetLanguageCookie(model.Language);

                if (model.Language != userCurrentLanguage)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction(nameof(AccountController.Login), new { alert = LoginAlert.LanguageChanged });
                }

                return RedirectToAction(nameof(HomeController.Overview), "Home", new { alert = OverviewAlert.ProfileUpdated });
            }

            return View(model);
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

            if (!new string[] { ".JPG", ".PNG", ".JPEG" }.Contains(extension.ToUpperInvariant()))
            {
                ModelState.AddModelError(string.Empty, _localizer["InvalidImageFormat"]);
                return new UnprocessableEntityObjectResult(ModelState);
            }

            string tempImageName = Guid.NewGuid() + extension;
            string tempImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "temp", tempImageName);

            if (image.Length > 0)
            {
                try
                {
                    using (var stream = new FileStream(tempImagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    ApplicationUser user = await _userManager.GetUserAsync(User);

                    string imageUri = await _cdnService.UploadProfileTempAsync(
                        user.Id,
                        filePath: tempImagePath,
                        uploadPath: $"users/{user.Id}",
                        template: "profile"
                    );

                    return StatusCode(201, new { imageUri });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(AccountController.UploadProfileImage));
                    throw;
                }
            }

            ModelState.AddModelError(string.Empty, _localizer["InvalidImage"]);
            return new UnprocessableEntityObjectResult(ModelState);
        }

        #region Helpers

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                string scheme = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (scheme != null && scheme != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(scheme);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = scheme;
                    }
                }
            }

            return vm;
        }

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
            var now = DateTime.UtcNow;
            var createMainAccount = new CreateMainAccount
            {
                UserId = userId,
                Name = _localizer["MainAccountName"]
            };
            await _accountService.CreateMainAsync(createMainAccount);
        }

        private async Task CreateSamplesAsync(int userId)
        {
            var sampleListTranslations = new Dictionary<string, string> {
                { "SampleListName", _localizer["SampleListName"] },
                { "SampleListTask1", _localizer["SampleListTask1"] },
                { "SampleListTask2", _localizer["SampleListTask2"] },
                { "SampleListTask3", _localizer["SampleListTask3"] }
            };
            await _listService.CreateSampleAsync(userId, sampleListTranslations);

            var sampleRecipeTranslations = new Dictionary<string, string> {
                { "SampleRecipeName", _localizer["SampleRecipeName"] },
                { "SampleRecipeDescription", _localizer["SampleRecipeDescription"] },
                { "SampleRecipeInstructions", _localizer["SampleRecipeInstructions"] },
                { "SampleRecipeIngredient1", _localizer["SampleRecipeIngredient1"] },
                { "SampleRecipeIngredient2", _localizer["SampleRecipeIngredient2"] },
                { "SampleRecipeIngredient3", _localizer["SampleRecipeIngredient3"] }
            };
            await _recipeService.CreateSampleAsync(userId, sampleRecipeTranslations);
        }

        private static LoginAlert GenerateLoginAlertFromRegistrationEmail(string email)
        {
            string domain = email.Substring(email.IndexOf('@', StringComparison.Ordinal) + 1);

            switch (domain.ToUpperInvariant())
            {
                case "GMAIL.COM":
                    return LoginAlert.RegistrationEmailSentGmail;
                case "OUTLOOK.COM":
                case "LIVE.COM":
                case "HOTMAIL.COM":
                    return LoginAlert.RegistrationEmailSentOutlook;
                case "YAHOO.COM":
                    return LoginAlert.RegistrationEmailSentYahoo;
                default:
                    return LoginAlert.RegistrationEmailSent;
            }
        }

        private static LoginAlert GenerateLoginAlertFromPasswordResetEmail(string email)
        {
            string domain = email.Substring(email.IndexOf('@', StringComparison.Ordinal) + 1);

            switch (domain.ToUpperInvariant())
            {
                case "GMAIL.COM":
                    return LoginAlert.PasswordResetEmailSentGmail;
                case "OUTLOOK.COM":
                case "LIVE.COM":
                case "HOTMAIL.COM":
                    return LoginAlert.PasswordResetEmailSentOutlook;
                case "YAHOO.COM":
                    return LoginAlert.PasswordResetEmailSentYahoo;
                default:
                    return LoginAlert.PasswordResetEmailSent;
            }
        }

        private void AddIdentityErrors(IdentityResult result, string prefix = "")
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, _localizer[prefix + error.Code]);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index));
            }
        }

        #endregion
    }
}
