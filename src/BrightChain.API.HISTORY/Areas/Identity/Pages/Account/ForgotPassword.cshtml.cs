﻿namespace BrightChain.API.Areas.Identity.Pages.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;

    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<BrightChainIdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<BrightChainIdentityUser> userManager, IEmailSender emailSender)
        {
            this._userManager = userManager;
            this._emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userManager.FindByEmailAsync(this.Input.Email).ConfigureAwait(false);
                if (user is null || !(await this._userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await this._userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = this.Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: this.Request.Scheme);

                await this._emailSender.SendEmailAsync(
                    this.Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait(false);

                return this.RedirectToPage("./ForgotPasswordConfirmation");
            }

            return this.Page();
        }
    }
}
