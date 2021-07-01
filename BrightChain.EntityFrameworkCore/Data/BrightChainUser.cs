using BrightChain.Models.Blocks;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrightChain.EntityFrameworkCore.Data
{
    public class BrightChainUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public IEnumerable<BrightChainBlock> Blocks { get; set; }
    }
}
