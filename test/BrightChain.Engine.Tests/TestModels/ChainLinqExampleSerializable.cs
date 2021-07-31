namespace BrightChain.Engine.Tests.TestModels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    public class ChainLinqExampleSerializable
        : ISerializable, IDisposable
    {
        public ChainLinqExampleSerializable()
        {
            this.TestData = new Bogus.DataSets.Lorem().Text();
        }

        protected ChainLinqExampleSerializable(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.TestData = info.GetString("TestData");
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

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TestData", this.TestData);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.GetObjectData(info, context);
        }

        public string TestData { get; }
    }
}
