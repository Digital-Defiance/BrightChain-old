namespace BrightChain.Engine.Tests.TestModels
{
    using System;
    using System.Collections.Generic;

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

        public string PublicData { get; }

        private string PrivateData { get; }
    }
}
