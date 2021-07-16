using System;
using System.ComponentModel.DataAnnotations;

namespace BrightChain.EntityFrameworkCore.Data.Entities
{
    public class BrightChainEntityBase
    {
        public BrightChainEntityBase()
        {
            //Id = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
        }

        [Key]
        public string Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
