using System;
using System.ComponentModel.DataAnnotations;

namespace BrightChain.Data.Entities
{
    public class Base
    {
        public Base() =>
            //Id = Guid.NewGuid();
            this.CreatedOn = DateTime.UtcNow;

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
