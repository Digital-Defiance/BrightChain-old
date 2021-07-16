using System;
using System.IO;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
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
            _configuration = new Mock<IConfiguration>().Object;
            _services = new Mock<IServiceCollection>().Object;
            _logger = new Mock<ILogger>().Object;

            var factoryMock = new Mock<ILoggerFactory>();

            factoryMock
                .SetupAllProperties()
                .Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_logger);

            _loggerFactory = factoryMock.Object;
        }

        [TestMethod]
        public void ItInitializesTest()
        {
            var loggerMock = Mock.Get(_logger);

            var brightChainService = new BrightBlockService(
                logger: _loggerFactory);

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

        [TestMethod]
        public void ItWhitensBlocksAndCreatesCblsTest()
        {
            var loggerMock = Mock.Get(_logger);

            var brightChainService = new BrightBlockService(
                logger: _loggerFactory);

            var fileName = Path.GetTempFileName();
            CreateRandomFile(fileName, 10);
            var cbl = brightChainService.CreateCblFromFile(
                fileName: fileName,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: false,
                privateEncrypted: false,
                blockSize: Enumerations.BlockSize.Medium
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
