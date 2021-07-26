using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BrightChain.API.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<BrightChainEntityUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public EnableAuthenticatorModel(
            UserManager<BrightChainEntityUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            this._userManager = userManager;
            this._logger = logger;
            this._urlEncoder = urlEncoder;
        }

        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await this._userManager.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            await this.LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(false);

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await this._userManager.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            if (!this.ModelState.IsValid)
            {
                await this.LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(false);
                return this.Page();
            }

            // Strip spaces and hypens
            var verificationCode = this.Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await this._userManager.VerifyTwoFactorTokenAsync(
                user, this._userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode).ConfigureAwait(false);

            if (!is2faTokenValid)
            {
                this.ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await this.LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(false);
                return this.Page();
            }

            await this._userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
            var userId = await this._userManager.GetUserIdAsync(user).ConfigureAwait(false);
            this._logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            this.StatusMessage = "Your authenticator app has been verified.";

            if (await this._userManager.CountRecoveryCodesAsync(user).ConfigureAwait(false) == 0)
            {
                var recoveryCodes = await this._userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10).ConfigureAwait(false);
                this.RecoveryCodes = recoveryCodes.ToArray();
                return this.RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return this.RedirectToPage("./TwoFactorAuthentication");
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(BrightChainEntityUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await this._userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await this._userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);
                unformattedKey = await this._userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);
            }

            this.SharedKey = this.FormatKey(unformattedKey);

            var email = await this._userManager.GetEmailAsync(user).ConfigureAwait(false);
            this.AuthenticatorUri = this.GenerateQrCodeUri(email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
AuthenticatorUriFormat,
this._urlEncoder.Encode("BrightChain.API"),
this._urlEncoder.Encode(email),
unformattedKey);
        }
    }
}
