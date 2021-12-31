namespace BrightChain.API.Infrastructure
{
    using System.Collections.Generic;
    using BrightChain.API.Areas.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;

    public class BrightChainRoleManager : RoleManager<BrightChainIdentityRole>
    {
        public BrightChainRoleManager(IRoleStore<BrightChainIdentityRole> store, IEnumerable<IRoleValidator<BrightChainIdentityRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<BrightChainIdentityRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }
    }
}
