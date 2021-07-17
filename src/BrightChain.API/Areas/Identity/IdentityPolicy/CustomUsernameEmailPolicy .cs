namespace BrightChain.API.Identity.IdentityPolicy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using Microsoft.AspNetCore.Identity;

    public class CustomUsernameEmailPolicy : UserValidator<BrightChainEntityUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<BrightChainEntityUser> manager, BrightChainEntityUser user)
        {
            IdentityResult result = await base.ValidateAsync(manager, user).ConfigureAwait(false);
            List<IdentityError> errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            if (user.UserName == "google")
            {
                errors.Add(new IdentityError
                {
                    Description = "Google cannot be used as a user name"
                });
            }

            /*if (!user.Email.ToLower().EndsWith("@yahoo.com"))
            {
                errors.Add(new IdentityError
                {
                    Description = "Only yahoo.com email addresses are allowed"
                });
            }*/
            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }
}
