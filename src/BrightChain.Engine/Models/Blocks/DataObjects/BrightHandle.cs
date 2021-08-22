namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System.Web;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Hashes;

    public struct BrightHandle
    {
        public readonly BlockSize BlockSize;
        public readonly IEnumerable<ReadOnlyMemory<byte>> BlockHashByteArrays;
        public readonly Type OriginalType;

        public BrightHandle(BlockSize blockSize, IEnumerable<BlockHash> blockHashes, Type originalType)
        {
            this.BlockSize = blockSize;
            this.BlockHashByteArrays = blockHashes.Select(h => h.HashBytes);
            this.OriginalType = originalType;
        }

        public BrightHandle(Uri brightChainUri)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockHash> BlockHashes
        {
            get
            {
                var blockSize = this.BlockSize;
                return this.BlockHashByteArrays.Select(r => new BlockHash(
                    typeof(BrightenedBlock),
                    originalBlockSize: blockSize,
                    providedHashBytes: r,
                    computed: true));
            }
        }

        public IEnumerable<string> HashStrings
        {
            get
            {
                return this.BlockHashByteArrays
                    .Select(r => Helpers.Utilities.HashToFormattedString(r.ToArray()));
            }
        }

        public Uri BrightChainAddress(string hostName)
        {
            UriBuilder uriBuilder = new UriBuilder(
                schemeName: "https",
                hostName: hostName);

            uriBuilder.Path = string.Format("/chains/{0}", this.BlockSize.ToString());

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("t", this.OriginalType.AssemblyQualifiedName);
            foreach (var s in this.HashStrings)
            {
                query.Add(name: null, value: s);
            }

            return uriBuilder.Uri;
        }
    }
}
