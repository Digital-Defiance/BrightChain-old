using System;
using BrightChain.Engine.Interfaces;
using ProtoBuf;

namespace BrightChain.Engine.Helpers;

/// <summary>
///     test object for the Faster Cache.
/// </summary>
[ProtoContract]
public class ProtoContractTestObject : object, ITransactable, IComparable<ProtoContractTestObject>
{
    [ProtoMember(tag: 1)] public string id;

    public ProtoContractTestObject(string id)
    {
        this.id = id;
    }

    public ProtoContractTestObject()
    {
        this.id = Guid.NewGuid().ToString();
    }

    public int CompareTo(ProtoContractTestObject other)
    {
        return string.Compare(strA: this.id,
            strB: other.id,
            comparisonType: StringComparison.Ordinal);
    }

    public void Commit()
    {
        throw new NotImplementedException();
    }

    public void Rollback()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        this.id = null;
    }
}
