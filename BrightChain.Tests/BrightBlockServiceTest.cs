using BrightChain.Models.Blocks.Chains;
using BrightChain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace BrightChain.Tests
{
    /// <summary>
    /// Exercises the core API service
    /// </summary>
    [TestClass]
    public class BrightBlockServiceTest
    {
        private ILoggerFactory _loggerFactory;
        private IConfiguration _configuration;
        private IServiceCollection _services;
        private ILogger _logger;

        [TestInitialize]
        public void PreTestSetup()
        {
            this._configuration = new Mock<IConfiguration>().Object;
            this._services = new Mock<IServiceCollection>().Object;
            this._logger = new Mock<ILogger>().Object;

            var factoryMock = new Mock<ILoggerFactory>();

            factoryMock
                .SetupAllProperties()
                .Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this._logger);

            this._loggerFactory = factoryMock.Object;
        }

        [TestMethod]
        public void ItInitializesTest()
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();
        }

        private static void CreateRandomFile(string filePath, int sizeInMb)
        {
            // Note: block size must be a factor of 1MB to avoid rounding errors
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;

            using (FileStream stream = File.OpenWrite(filePath))
            {
                for (int i = 0; i < sizeInMb * blocksPerMb; i++)
                {
                    var data = Helpers.RandomDataHelper.RandomReadOnlyBytes(blockSize);
                    stream.Write(data.ToArray(), 0, blockSize);
                }
            }
        }

        public async void ItWhitensBlocksAndCreatesCblsTest()
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory);

            var fileName = Path.GetTempFileName();
            var cbl = await brightChainService.CreateCblFromFile(
                fileName: fileName,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: false,
                privateEncrypted: false,
                blockSize: Enumerations.BlockSize.Tiny
            );
            Assert.IsTrue(cbl.Validate());

            var cblMap = cbl.BlockMap;
            Assert.IsTrue(cblMap is BlockChainFileMap);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
