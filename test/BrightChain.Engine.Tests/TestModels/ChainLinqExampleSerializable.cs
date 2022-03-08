using System;
using System.Collections.Generic;
using Bogus.DataSets;
using ProtoBuf;

namespace BrightChain.Engine.Tests.TestModels;

[ProtoContract]
public class ChainLinqExampleSerializable
    : IDisposable
{
    public ChainLinqExampleSerializable()
    {
        this.PublicData = new Lorem().Text();
        this.PrivateData = new Lorem().Text();
    }

    [ProtoMember(tag: 1)] public string PublicData { get; }

    [ProtoMember(tag: 2)] private string PrivateData { get; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<ChainLinqExampleSerializable> MakeMultiple(int count)
    {
        var datas = new ChainLinqExampleSerializable[count];
        for (var i = 0; i < count; i++)
        {
            datas[i] = new ChainLinqExampleSerializable();
        }

        return datas;
    }
}
