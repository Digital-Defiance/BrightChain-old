namespace BrightChain.API.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BrightChain.API.Areas.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NETCore.Encrypt;

    /// <summary>
    /// Custom UserManager to override Authenticator Token generation behavior (encrypt/decrypt).
    /// from https://chsakell.com/2019/08/18/asp-net-core-identity-series-two-factor-authentication/
    /// </summary>
    public class BrightChainUserManager : UserManager<BrightChainIdentityUser>
    {
        private readonly IConfiguration _configuration;

        public BrightChainUserManager(
            IUserStore<BrightChainIdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<BrightChainIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<BrightChainIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<BrightChainIdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<BrightChainIdentityUser>> logger,
            IConfiguration configuration)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
                keyNormalizer, errors, services, logger)
        {
            this._configuration = configuration;
        }

        #region Authenticator App key

        public override string GenerateNewAuthenticatorKey()
        {
            var originalAuthenticatorKey = base.GenerateNewAuthenticatorKey();

            var aesKey = EncryptProvider.CreateAesKey();

            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            var encryptedKey = encryptionEnabled
                ? EncryptProvider.AESEncrypt(originalAuthenticatorKey, this._configuration["TwoFactorAuthentication:EncryptionKey"])
                : originalAuthenticatorKey;

            return encryptedKey;
        }

        public override async Task<string> GetAuthenticatorKeyAsync(BrightChainIdentityUser user)
        {
            var databaseKey = await base.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);

            if (databaseKey == null)
            {
                return null;
            }

            // Decryption
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            var originalAuthenticatorKey = encryptionEnabled
                ? EncryptProvider.AESDecrypt(databaseKey, this._configuration["TwoFactorAuthentication:EncryptionKey"])
                : databaseKey;

            return originalAuthenticatorKey;
        }

        #endregion

        #region Recovery codes

        protected override string CreateTwoFactorRecoveryCode()
        {
            var originalRecoveryCode = base.CreateTwoFactorRecoveryCode();

            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            var encryptedRecoveryCode = encryptionEnabled
                ? EncryptProvider.AESEncrypt(originalRecoveryCode, this._configuration["TwoFactorAuthentication:EncryptionKey"])
                : originalRecoveryCode;

            return encryptedRecoveryCode;
        }

        public override async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(BrightChainIdentityUser user, int number)
        {
            var tokens = await base.GenerateNewTwoFactorRecoveryCodesAsync(user, number).ConfigureAwait(false);

            var generatedTokens = tokens as string[] ?? tokens.ToArray();
            if (!generatedTokens.Any())
            {
                return generatedTokens;
            }

            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            return encryptionEnabled
                ? generatedTokens
                    .Select(token =>
                        EncryptProvider.AESDecrypt(token, this._configuration["TwoFactorAuthentication:EncryptionKey"]))
                : generatedTokens;
        }

        public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(BrightChainIdentityUser user, string code)
        {
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            if (encryptionEnabled && !string.IsNullOrEmpty(code))
            {
                code = EncryptProvider.AESEncrypt(code, this._configuration["TwoFactorAuthentication:EncryptionKey"]);
            }

            return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
        }

        #endregion

    }
}
