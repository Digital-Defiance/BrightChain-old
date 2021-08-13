namespace BrightChain.Engine.Tests.TestModels
{
    using System;
    using System.Collections.Generic;
    using ProtoBuf;

    [ProtoContract]
    public class ChainLinqExampleSerializable
        : IDisposable
    {
        public ChainLinqExampleSerializable()
        {
            this.PublicData = new Bogus.DataSets.Lorem().Text();
            this.PrivateData = new Bogus.DataSets.Lorem().Text();
        }

        public static IEnumerable<ChainLinqExampleSerializable> MakeMultiple(int count)
        {
            ChainLinqExampleSerializable[] datas = new ChainLinqExampleSerializable[count];
            for (int i = 0; i < count; i++)
            {
                datas[i] = new ChainLinqExampleSerializable();
            }

            return datas;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [ProtoMember(1)]
        public string PublicData { get; }

        [ProtoMember(2)]
        private string PrivateData { get; }
    }
}
