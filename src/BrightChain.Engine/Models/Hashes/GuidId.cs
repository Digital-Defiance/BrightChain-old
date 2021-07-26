using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Blocks;

namespace BrightChain.Engine.Models.Hashes
{
    public class GuidId : DataHash
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public const int HashSize = 128;

        /// <summary>
        /// Size in bytes of the hash.
        /// </summary>
        public const int HashSizeBytes = HashSize / 8;

        public readonly Guid Guid;

        public GuidId(Guid guid, long sourceDataLength)
            : base(providedHashBytes: guid.ToByteArray(), sourceDataLength: sourceDataLength, computed: false)
        {
            this.Guid = guid;
        }
    }
}
