namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System.Web;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    [ProtoContract]
    public struct BrightHandle
    {
        [ProtoMember(1)]
        public readonly BlockSize BlockSize;

        [ProtoMember(2)]
        public readonly IEnumerable<ReadOnlyMemory<byte>> BlockHashByteArrays;

        [ProtoMember(3)]
        public readonly Type OriginalType;

        [ProtoMember(4)]
        public readonly BlockHash BrightenedCblHash;

        [ProtoMember(5)]
        public readonly DataHash IdentifiableSourceHash;

        public BrightHandle(BlockSize blockSize, IEnumerable<BlockHash> blockHashes, Type originalType, BlockHash brightenedCblHash = null, DataHash identifiableSourceHash = null)
        {
            this.BlockSize = blockSize;
            this.BlockHashByteArrays = blockHashes.Select(h => h.HashBytes);
            this.OriginalType = originalType;
            this.BrightenedCblHash = brightenedCblHash;
            this.IdentifiableSourceHash = identifiableSourceHash;
        }

        public BrightHandle(Uri brightChainAddress)
        {
            throw new NotImplementedException();
        }

        public int TupleCount => this.BlockHashByteArrays.Count();

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

        public Uri BrightChainAddress(string hostName, string endpoint = "chains")
        {
            UriBuilder uriBuilder = new UriBuilder(
                schemeName: "https",
                hostName: hostName);

            uriBuilder.Path = string.Format("/{0}/{1}", endpoint, this.BlockSize.ToString());

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("t", this.OriginalType.AssemblyQualifiedName);
            foreach (var s in this.HashStrings)
            {
                query.Add(name: null, value: s);
            }

            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }
    }
}
