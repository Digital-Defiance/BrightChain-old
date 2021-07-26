using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrightChain.API.Infrastructure
{
    /// <summary>
    /// Custom UserManager to override Authenticator Token generation behavior (encrypt/decrypt)
    /// </summary>
    public class BrightChainUserManager : UserManager<BrightChainEntityUser>
    {
        private readonly IConfiguration _configuration;

        public BrightChainUserManager(IUserStore<BrightChainEntityUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<BrightChainEntityUser> passwordHasher, IEnumerable<IUserValidator<BrightChainEntityUser>> userValidators,
            IEnumerable<IPasswordValidator<BrightChainEntityUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<BrightChainEntityUser>> logger,
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

            throw new NotImplementedException();
            // var aesKey = EncryptProvider.CreateAesKey();

            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            //var encryptedKey = encryptionEnabled
            //    ? EncryptProvider.AESEncrypt(originalAuthenticatorKey, _configuration["TwoFactorAuthentication:EncryptionKey"])
            //    : originalAuthenticatorKey;

            //return encryptedKey;
        }

        public override async Task<string> GetAuthenticatorKeyAsync(BrightChainEntityUser user)
        {
            var databaseKey = await base.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);

            if (databaseKey == null)
            {
                return null;
            }
            throw new NotImplementedException();

            // Decryption
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            //var originalAuthenticatorKey = encryptionEnabled
            //    ? EncryptProvider.AESDecrypt(databaseKey, _configuration["TwoFactorAuthentication:EncryptionKey"])
            //    : databaseKey;

            //return originalAuthenticatorKey;
        }

        #endregion

        #region Recovery codes

        protected override string CreateTwoFactorRecoveryCode()
        {
            var originalRecoveryCode = base.CreateTwoFactorRecoveryCode();

            throw new NotImplementedException();
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            //var encryptedRecoveryCode = encryptionEnabled
            //    ? EncryptProvider.AESEncrypt(originalRecoveryCode, _configuration["TwoFactorAuthentication:EncryptionKey"])
            //    : originalRecoveryCode;

            //return encryptedRecoveryCode;
        }

        public override async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(BrightChainEntityUser user, int number)
        {
            var tokens = await base.GenerateNewTwoFactorRecoveryCodesAsync(user, number).ConfigureAwait(false);

            var generatedTokens = tokens as string[] ?? tokens.ToArray();
            if (!generatedTokens.Any())
            {
                return generatedTokens;
            }

            throw new NotImplementedException();
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            //return encryptionEnabled
            //    ? generatedTokens
            //        .Select(token =>
            //            EncryptProvider.AESDecrypt(token, _configuration["TwoFactorAuthentication:EncryptionKey"]))
            //    : generatedTokens;

        }

        public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(BrightChainEntityUser user, string code)
        {
            bool.TryParse(this._configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

            if (encryptionEnabled && !string.IsNullOrEmpty(code))
            {
                //code = EncryptProvider.AESEncrypt(code, _configuration["TwoFactorAuthentication:EncryptionKey"]);
                throw new NotImplementedException();
            }

            return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
        }

        #endregion

    }
}
