using BrightChain.Engine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests;

[TestClass]
public class BrightChainKeyServiceTest
{
    private readonly IConfiguration configuration;

    //private BlockCacheManager blockCacheManager;
    private ILogger logger;

    [TestInitialize]
    public void SetUp()
    {
        this.logger = new Mock<ILogger>().Object;
        var mockConfiguration = new Mock<IConfiguration>();

        //Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
        //mockSection.Setup(x => x.Value).Returns(Path.GetTempPath());
        //mockConfiguration.Setup(x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(mockSection.Object);
        //this.configuration = new Mock<IConfiguration>().Object;
        // the cache manager under test
        //var mockCacheManager = new Mock<BlockCacheManager>(this.logger, this.configuration);
        //this.blockCacheManager = mockCacheManager.Object;
    }

    [TestMethod]
    public void ItLoadsPrivateKeysTest()
    {
        const string privateKey = "c711e5080f2b58260fe19741a7913e8301c1128ec8e80b8009406e5047e6e1ef";
        var privateECDsa = BrightChainKeyService.LoadPrivateKey(hexKeyString: privateKey);
        Assert.IsNotNull(value: privateECDsa);
    }

    [TestMethod]
    public void ItLoadsPublicKeysTest()
    {
        const string publicKey =
            "04e33993f0210a4973a94c26667007d1b56fe886e8b3c2afdd66aa9e4937478ad20acfbdc666e3cec3510ce85d40365fc2045e5adb7e675198cf57c6638efa1bdb";
        var publicECDsa = BrightChainKeyService.LoadPublicKey(hexKeyString: publicKey);
        Assert.IsNotNull(value: publicECDsa);
    }

    [TestMethod]
    public void ItValidatesJwtTest()
    {
        const string privateKey = "c711e5080f2b58260fe19741a7913e8301c1128ec8e80b8009406e5047e6e1ef";
        const string publicKey =
            "04e33993f0210a4973a94c26667007d1b56fe886e8b3c2afdd66aa9e4937478ad20acfbdc666e3cec3510ce85d40365fc2045e5adb7e675198cf57c6638efa1bdb";

        var privateECDsa = BrightChainKeyService.LoadPrivateKey(hexKeyString: privateKey);
        var publicECDsa = BrightChainKeyService.LoadPublicKey(hexKeyString: publicKey);

        var jwt = BrightChainKeyService.CreateSignedJwt(eCDsa: privateECDsa,
            audience: "user1234");
        var isValid = BrightChainKeyService.VerifySignedJwt(eCDsa: publicECDsa,
            token: jwt,
            audience: "user1234");
        Assert.IsTrue(condition: isValid);
    }
}
