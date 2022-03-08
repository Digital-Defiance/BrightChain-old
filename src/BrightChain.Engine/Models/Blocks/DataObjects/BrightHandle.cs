using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Hashes;
using NeuralFabric.Helpers;
using NeuralFabric.Models.Hashes;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

[ProtoContract]
public struct BrightHandle
{
    [ProtoMember(tag: 1)] public readonly BlockSize BlockSize;

    [ProtoMember(tag: 2)] public readonly IEnumerable<ReadOnlyMemory<byte>> BlockHashByteArrays;

    [ProtoMember(tag: 3)] public readonly Type OriginalType;

    [ProtoMember(tag: 4)] public readonly BlockHash BrightenedCblHash;

    [ProtoMember(tag: 5)] public readonly DataHash IdentifiableSourceHash;

    public BrightHandle(BlockSize blockSize, IEnumerable<BlockHash> blockHashes, Type originalType, BlockHash brightenedCblHash = null,
        DataHash identifiableSourceHash = null)
    {
        this.BlockSize = blockSize;
        this.BlockHashByteArrays = blockHashes.Select(selector: h => h.HashBytes);
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
            return this.BlockHashByteArrays.Select(selector: r => new BlockHash(
                blockType: typeof(BrightenedBlock),
                originalBlockSize: blockSize,
                providedHashBytes: r,
                computed: true));
        }
    }

    public IEnumerable<string> HashStrings => this.BlockHashByteArrays
        .Select(selector: r => Utilities.HashToFormattedString(hashBytes: r.ToArray()));

    public Uri BrightChainAddress(string hostName, string endpoint = "chains")
    {
        var uriBuilder = new UriBuilder(
            schemeName: "https",
            hostName: hostName);

        uriBuilder.Path = string.Format(format: "/{0}/{1}",
            arg0: endpoint,
            arg1: this.BlockSize.ToString());

        var query = HttpUtility.ParseQueryString(query: uriBuilder.Query);
        query.Add(name: "t",
            value: this.OriginalType.AssemblyQualifiedName);
        foreach (var s in this.HashStrings)
        {
            query.Add(name: null,
                value: s);
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }
}
