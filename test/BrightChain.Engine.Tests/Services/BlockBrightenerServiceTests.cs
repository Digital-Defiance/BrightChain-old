namespace BrightChain.Engine.Tests.Services
{
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Services;
    using BrightChain.Engine.Services.CacheManagers.Block;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;

    [TestClass]
    public class BlockBrightenerServiceTests
    {
        private MockRepository mockRepository;

        private Mock<BrightenedBlockCacheManagerBase> mockBrightenedBlockCacheManagerBase;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockBrightenedBlockCacheManagerBase = this.mockRepository.Create<BrightenedBlockCacheManagerBase>();
        }

        private BlockBrightenerService CreateService()
        {
            return new BlockBrightenerService(
                this.mockBrightenedBlockCacheManagerBase.Object);
        }

        [TestMethod]
        public void Brighten_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            IdentifiableBlock identifiableBlock = null;
            BrightenedBlock[] randomizersUsed = null;
            TupleStripe brightenedStripe = default(global::BrightChain.Engine.Models.Blocks.Chains.TupleStripe);

            // Act
            var result = service.Brighten(
                identifiableBlock,
                out randomizersUsed,
                out brightenedStripe);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
