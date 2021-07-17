namespace BrightChain.API.Identity.IdentityPolicy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using Microsoft.AspNetCore.Identity;

    public class CustomPasswordPolicy : PasswordValidator<BrightChainEntityUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<BrightChainEntityUser> manager, BrightChainEntityUser user, string password)
        {
            IdentityResult result = await base.ValidateAsync(manager, user, password).ConfigureAwait(false);
            List<IdentityError> errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            if (password.ToLower(culture: System.Globalization.CultureInfo.InvariantCulture).Contains(user.UserName.ToLower(culture: System.Globalization.CultureInfo.InvariantCulture)))
            {
                errors.Add(new IdentityError
                {
                    Description = "Password cannot contain username"
                });
            }
            if (password.Contains("123"))
            {
                errors.Add(new IdentityError
                {
                    Description = "Password cannot contain 123 numeric sequence"
                });
            }
            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }
}
