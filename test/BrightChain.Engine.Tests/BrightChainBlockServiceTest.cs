using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
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
    public class BrightChainBlockServiceTest
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

            _loggerFactory = factoryMock.Object;
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

        /// <summary>
        /// TODO: move to BlockHashTests.
        /// </summary>
        [TestMethod]
        public void ItHasCorrectHashSizesTest()
        {
            foreach (BlockSize blockSize in Enum.GetValues(typeof(BlockSize)))
            {
                if (blockSize == BlockSize.Unknown)
                {
                    continue;
                }

                var expectedVector = Helpers.Utilities.GetZeroVector(blockSize);
                BlockHash zeroVector;
                Helpers.Utilities.GenerateZeroVectorAndVerify(blockSize, out zeroVector);
                Assert.IsNotNull(zeroVector);
                Assert.AreEqual(expectedVector.ToString(), zeroVector.ToString());
            }
        }

        public static BlockSize RandomBlockSize()
        {
            Array values = Enum.GetValues(typeof(BlockSize));
            Random random = new Random();
            var blockSize = (BlockSize)values.GetValue(random.Next(values.Length));
            return (blockSize == BlockSize.Unknown) ? RandomBlockSize() : blockSize;
        }

        private static long CreateRandomFile(string filePath, int sizeInMb, out byte[] randomFileHash, int sizeOffset = 0)
        {
            const int writeBufferSize = 1024 * 8;
            int totalBytes = (sizeInMb * 1024 * 1024) - sizeOffset;

            if ((sizeOffset < 0) || (sizeOffset > writeBufferSize))
            {
                throw new Exception(nameof(sizeOffset));
            }

            var bytesWritten = 0;
            var bytesRemaining = totalBytes;
            using (SHA256 sha = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenWrite(filePath))
                {
                    while (bytesWritten < totalBytes)
                    {
                        var finalBlock = bytesRemaining <= writeBufferSize;
                        var lengthToWrite = finalBlock ? writeBufferSize - sizeOffset : writeBufferSize;
                        var data = Helpers.RandomDataHelper.RandomBytes(lengthToWrite);
                        Assert.AreEqual(lengthToWrite, data.Length);
                        fileStream.Write(data, 0, data.Length);
                        bytesWritten += data.Length;
                        bytesRemaining -= data.Length;
                        if (finalBlock)
                        {
                            Assert.AreEqual(0, bytesRemaining);
                            sha.TransformFinalBlock(data, 0, data.Length);
                            randomFileHash = sha.Hash;
                            fileStream.Flush();
                            fileStream.Close();
                            FileInfo fileInfo = new FileInfo(filePath);
                            Assert.AreEqual(bytesWritten, fileInfo.Length);
                            Assert.AreEqual(totalBytes, bytesWritten);

                            return bytesWritten;
                        }
                        else
                        {
                            sha.TransformBlock(data, 0, lengthToWrite, null, 0);
                        }
                    }
                }
            }

            randomFileHash = null;
            return -1;
        }

        [TestMethod]

        public async Task ItBrightensBlocksAndCreatesCblsTest()
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory);

            var fileName = Path.GetTempFileName();
            byte[] sourceFileHash;
            long expectedLength = CreateRandomFile(fileName, 10, out sourceFileHash, 3); // don't land on even block mark for data testing

            ConstituentBlockListBlock[] cblBlocks = (ConstituentBlockListBlock[])await brightChainService.MakeCBLChainFromParamsAsync(
                fileName: fileName,
                blockParams: new BlockParams(
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    blockSize: RandomBlockSize()));

            foreach (var cbl in cblBlocks)
            {
                Assert.IsTrue(cbl.Validate());
                Assert.AreEqual(expectedLength, cbl.TotalLength);
                Assert.AreEqual(
                    Helpers.Utilities.HashToFormattedString(sourceFileHash),
                    Helpers.Utilities.HashToFormattedString(cbl.SourceId.HashBytes.ToArray()));

                var cblMap = cbl.GenerateBlockMap();
                Assert.IsTrue(cblMap is BlockChainFileMap);
            }

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ItReadsCBLsBackToDisk()
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory);

            var fileName = Path.GetTempFileName();
            byte[] sourceFileHash;
            long expectedLength = CreateRandomFile(fileName, 10, out sourceFileHash, 3); // don't land on even block mark for data testing

            var blockSize = RandomBlockSize();
            ConstituentBlockListBlock[] cblBlocks = (ConstituentBlockListBlock[])await brightChainService.MakeCBLChainFromParamsAsync(
                fileName: fileName,
                blockParams: new BlockParams(
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    blockSize: blockSize));

            foreach (var cbl in cblBlocks)
            {
                Assert.IsTrue(cbl.Validate());

                Assert.AreEqual(expectedLength, cbl.TotalLength);
                Assert.AreEqual(
                    Helpers.Utilities.HashToFormattedString(sourceFileHash),
                    Helpers.Utilities.HashToFormattedString(cbl.SourceId.HashBytes.ToArray()));
            }
            // this is off- can't just give the last block.
            throw new NotImplementedException();
            var restoredFile = await brightChainService.RestoreFileFromCBLAsync(cblBlocks[cblBlocks.Length - 1]);

            Assert.AreEqual(
                Helpers.Utilities.HashToFormattedString(sourceFileHash),
                Helpers.Utilities.HashToFormattedString(restoredFile.SourceId.HashBytes.ToArray()));

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
