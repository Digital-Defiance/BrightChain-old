using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Data
{
    public class BrightChainBlock
    {
        [Required]
        public string Id { get; set; }
        [Required] 
        public string Data { get; set; }
        [Required]
        public string Metadata { get; set; }
 
        public Block ToBlock()
        {
            throw new NotImplementedException();
        }

        public static BrightChainBlock FromBrightChainBlock(Block block)
        {
            throw new NotImplementedException();
        }
    }
}
