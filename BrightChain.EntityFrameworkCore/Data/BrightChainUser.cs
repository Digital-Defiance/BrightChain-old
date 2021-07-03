using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrightChain.EntityFrameworkCore.Data
{
    /// <summary>
    /// Basic BrightChain user representation
    /// </summary>
    public class BrightChainUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

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
