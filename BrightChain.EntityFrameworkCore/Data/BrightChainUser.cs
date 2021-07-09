using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BrightChain.EntityFrameworkCore.Data
{
    /// <summary>
    /// Basic BrightChain user representation
    /// </summary>
    public class BrightChainUser : IdentityUser
    {
        /// <summary>
        /// Blocks whose CBLs are stored in-chain
        /// </summary>
        public IEnumerable<BrightChainBlock> PublicBlocks { get; set; }

        /// <summary>
        /// TODO: decide type and implement.
        /// </summary>
        public IEnumerable<BrightChainBlock> PrivateBlocks { get; set; }
    }
}
