namespace BrightChain.API.Infrastructure
{
    using BrightChain.API.Areas.Identity;
    using BrightChain.API.Identity.Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class BrightChainRoleStore : RoleStore<BrightChainIdentityRole>
    {
        public BrightChainRoleStore(BrightChainIdentityDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }
}
