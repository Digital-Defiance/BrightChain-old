namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System;

    public class StoredBlockData : BlockData
    {
        private readonly ReadOnlyMemory<byte> StoredBytes;

        public override ReadOnlyMemory<byte> Bytes
        {
            get
                => this.StoredBytes;
        }

        public StoredBlockData(ReadOnlyMemory<byte> data)
        {
            this.StoredBytes = data;
        }
    }
}
