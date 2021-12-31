// <copyright file="ShowRecoveryCodes.cshtml.cs" company="BrightChain">
// Copyright (c) BrightChain. All rights reserved.
// </copyright>

namespace BrightChain.API.Areas.Identity.Pages.Account.Manage
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class ShowRecoveryCodesModel : PageModel
    {
        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult OnGet()
        {
            if (this.RecoveryCodes is null || this.RecoveryCodes.Length == 0)
            {
                return this.RedirectToPage("./TwoFactorAuthentication");
            }

            return this.Page();
        }
    }
}
