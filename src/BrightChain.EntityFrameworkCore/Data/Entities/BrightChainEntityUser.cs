namespace BrightChain.EntityFrameworkCore.Data.Entities
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Basic BrightChain user representation
    /// </summary>
    public class BrightChainEntityUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets Blocks whose CBLs are stored in-chain
        /// </summary>
        public IEnumerable<BrightChainEntityBlock> PublicBlocks { get; set; }

        /// <summary>
        /// Gets or sets Blocks whose CBLs and data are stored encrypted
        /// </summary>
        public IEnumerable<BrightChainEntityBlock> PrivateBlocks { get; set; }
    }
}
