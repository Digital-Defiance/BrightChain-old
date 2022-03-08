using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services.CacheManagers.Block;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests;

[TestClass]
public class BlockValidatorExtensionsTest
{
    protected ILogger logger { get; set; }

    [TestInitialize]
    public void PreTestSetUp()
    {
        this.logger = new Mock<ILogger<BrightenedBlockCacheManagerBase>>().Object;
    }

    [DataTestMethod]
    [DataRow(data1: BlockSize.Nano)]
    [DataRow(data1: BlockSize.Micro)]
    [DataRow(data1: BlockSize.Message)]
    [DataRow(data1: BlockSize.Tiny)]
    [DataRow(data1: BlockSize.Small)]
    [DataRow(data1: BlockSize.Medium)]
    [DataRow(data1: BlockSize.Large)]
    public void ItValidatesValidBlocksTest(BlockSize blockSize)
    {
        Assert.IsTrue(condition: new ZeroVectorBlock(
                blockParams: new BlockParams(
                    blockSize: blockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    originalType: typeof(ZeroVectorBlock)))
            .Validate());

        var loggerMock = Mock.Get(mocked: this.logger);
        loggerMock.Verify(expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 0));
        loggerMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    [Ignore]
    public void ItValidatesUnknownBlockSizeTest()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void ItValidatesBlockSizeMatchesDataSizeTest()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void ItValidatesBlockHashMatchesBlockHashTest()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void ItValidatesStorageContractDataLengthTest()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void ItValidatesStorageContractMatchesRedundancyContractTest()
    {
        throw new NotImplementedException();
    }
}
