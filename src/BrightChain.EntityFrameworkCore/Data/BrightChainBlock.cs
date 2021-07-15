using BrightChain.Models.Blocks;
using System;
using System.ComponentModel.DataAnnotations;

namespace BrightChain.EntityFrameworkCore.Data
{
    /// <summary>
    /// Stripped model representation of Block class compatible with EntityFramework. Essentially a "rendered block". Strings are raw binary.
    /// </summary>
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
