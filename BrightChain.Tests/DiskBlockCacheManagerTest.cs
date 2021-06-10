using BrightChain.Enumerations;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrightChain.Tests
{
    public class DiskCacheTestBlock : Block
    {

        public DiskCacheTestBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) : base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {

        }

        public static ReadOnlyMemory<byte> NewRandomData()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)random.Next(0, 255);
            return new ReadOnlyMemory<byte>(data);
        }

        //public static DiskCacheTestBlock NewTestBlock() =>
        //    new DiskCacheTestBlock(
        //        requestTime: DateTime.Now,
        //        keepUntilAtLeast: DateTime.MaxValue,
        //        redundancy: Enumerations.RedundancyContractType.LocalNone,
        //        data: NewRandomData());

        public DiskCacheTestBlock() : base(requestTime: DateTime.Now, keepUntilAtLeast: DateTime.MaxValue, redundancy: RedundancyContractType.LocalNone, data: NewRandomData())
        {

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }
    }

    public class TestBlockSerializer : ISerializer<DiskCacheTestBlock>
    {
        private BlockSerializer blockSerializer = new BlockSerializer();

        public DiskCacheTestBlock ReadFrom(Stream stream)
        {
            return (DiskCacheTestBlock)blockSerializer.ReadFrom(stream);
        }

        public void WriteTo(DiskCacheTestBlock value, Stream stream)
        {

        }

    }

    [TestClass]
    public class DiskBlockCacheManagerTest : CacheManagerTest<DiskCacheManager<BlockHash, DiskCacheTestBlock>, BlockHash, DiskCacheTestBlock>
    {
        public BPlusTree<BlockHash, DiskCacheTestBlock>.OptionsV2 DefaultOptions()
        {
            BPlusTree<BlockHash, DiskCacheTestBlock>.OptionsV2 options = new BPlusTree<BlockHash, DiskCacheTestBlock>.OptionsV2(
                keySerializer: new BlockHashSerializer(),
                valueSerializer: new TestBlockSerializer());
            return options;
        }

        internal override ICacheManager<BlockHash, DiskCacheTestBlock> NewCacheManager(ILogger logger) => new DiskCacheManager<BlockHash, DiskCacheTestBlock>(logger: logger, optionsV2: DefaultOptions());

        internal override KeyValuePair<BlockHash, DiskCacheTestBlock> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < BlockSizeMap.BlockSize(BlockSize.Message); i++)
                data[i] = (byte)random.Next(0, 255);
            var block = new DiskCacheTestBlock(
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.LocalNone,
                data: data);
            return new KeyValuePair<BlockHash, DiskCacheTestBlock>(block.Id, block);
        }

        internal override DiskCacheTestBlock NewNullData() => null;

        [TestMethod]
        public void TestSetGetIntegrity()
        {
            // Arrange
            var expectation = testPair.Value;
            cacheManager.Set(testPair.Key, expectation);

            // Act
            DiskCacheTestBlock result = cacheManager.Get(testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
            Assert.AreSame(expectation, result);
            Assert.AreEqual(expectation.Data, result.Data);
        }
    }
}
