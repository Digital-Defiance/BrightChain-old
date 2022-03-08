using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Services;
using BrightChain.Engine.Services.CacheManagers.Block;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests.Services;

[TestClass]
public class BlockBrightenerServiceTests
{
    private Mock<BrightenedBlockCacheManagerBase> mockBrightenedBlockCacheManagerBase;
    private MockRepository mockRepository;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(defaultBehavior: MockBehavior.Strict);

        this.mockBrightenedBlockCacheManagerBase = this.mockRepository.Create<BrightenedBlockCacheManagerBase>();
    }

    private BlockBrightenerService CreateService()
    {
        return new BlockBrightenerService(
            resultCache: this.mockBrightenedBlockCacheManagerBase.Object);
    }

    [TestMethod]
    public void Brighten_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        IdentifiableBlock identifiableBlock = null;
        BrightenedBlock[] randomizersUsed = null;
        var brightenedStripe = default(TupleStripe);

        // Act
        var result = service.Brighten(
            identifiableBlock: identifiableBlock,
            randomizersUsed: out randomizersUsed,
            brightenedStripe: out brightenedStripe);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }
}
