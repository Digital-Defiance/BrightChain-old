using BrightChain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BrightChain.Tests
{
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
            this._loggerFactory = new Mock<ILoggerFactory>().Object;
            this._logger = new Mock<ILogger>().Object;
            this._configuration = new Mock<IConfiguration>().Object;
            this._services = new Mock<IServiceCollection>().Object;

            Mock.Get<ILoggerFactory>(this._loggerFactory)
                .SetupAllProperties()
                .Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this._logger);
        }

        [TestMethod]
        public void ServiceInitializionTest()
        {
            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory,
                configuration: this._configuration,
                services: this._services);

            var mock = Mock.Get(this._logger);

            mock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(4));
            mock.VerifyNoOtherCalls();
        }
    }
}
