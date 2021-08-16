
namespace BrightChain.Engine.Helpers
{
    using BrightChain.Engine.Interfaces;
    using ProtoBuf;

    /// <summary>
    /// test object for the Faster Cache.
    /// </summary>
    [ProtoContract]
    public class ProtoContractTestObject : object, ITransactable, IComparable<ProtoContractTestObject>
    {
        [ProtoMember(1)]
        public string id;

        public ProtoContractTestObject(string id)
        {
            this.id = id;
        }

        public ProtoContractTestObject()
        {
            this.id = Guid.NewGuid().ToString();
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

        public int CompareTo(ProtoContractTestObject other)
        {
            return string.Compare(this.id, other.id, StringComparison.Ordinal);
        }
    }
}
