namespace BrightChain.EntityFrameworkCore.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using BrightChain.Engine.Models.Blocks;

    /// <summary>
    /// Stripped model representation of Block class compatible with EntityFramework. Essentially a "rendered block". Strings are raw binary.
    /// maybe base64?
    /// </summary>
    public class BrightChainEntityBlock : BrightChainEntityBase
    {
        [Required]
        public string Data { get; set; }

        [Required]
        public string Metadata { get; set; }

        public Block ToBlock()
        {
            throw new NotImplementedException();
        }

        public static BrightChainEntityBlock FromBrightChainBlock(Block block)
        {
            throw new NotImplementedException();
        }
    }
}
