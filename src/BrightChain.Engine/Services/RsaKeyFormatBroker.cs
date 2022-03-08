using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace BrightChain.Engine.Services;

/// <summary>
///     from: https://stackoverflow.com/questions/15629551/read-rsa-privatekey-in-c-sharp-and-bouncy-castle
/// </summary>
public static class RsaKeyFormatBroker
{
    public static JwtSecurityToken GenerateJWTToken(SigningCredentials rsaPrivateKey, string audience)
    {
        return new JwtSecurityToken(
            issuer: "BrightChain",
            audience: audience,
            claims: null,
            notBefore: null,
            expires: null,
            signingCredentials: rsaPrivateKey);
    }

    private static SecurityKey GetSymmetricSecurityKey(byte[] symmetricKey)
    {
        return new SymmetricSecurityKey(key: symmetricKey);
    }

    private static RSAParameters GetRsaParameters(string rsaPrivateKey)
    {
        var byteArray = Encoding.ASCII.GetBytes(s: rsaPrivateKey);
        using (var ms = new MemoryStream(buffer: byteArray))
        {
            using (var sr = new StreamReader(stream: ms))
            {
                // use Bouncy Castle to convert the private key to RSA parameters
                var pemReader = new PemReader(reader: sr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                return DotNetUtilities.ToRSAParameters(privKey: keyPair.Private as RsaPrivateCrtKeyParameters);
            }
        }
    }

    /// <summary>
    ///     Import OpenSSH PEM private key string into MS RSACryptoServiceProvider.
    /// </summary>
    /// <param name="pem"></param>
    /// <returns></returns>
    public static RSACryptoServiceProvider ImportPrivateKey(string pem)
    {
        var pr = new PemReader(reader: new StringReader(s: pem));
        var KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
        var rsaParams = DotNetUtilities.ToRSAParameters(privKey: (RsaPrivateCrtKeyParameters)KeyPair.Private);

        var csp = new RSACryptoServiceProvider(); // cspParams);
        csp.ImportParameters(parameters: rsaParams);
        return csp;
    }

    /// <summary>
    ///     Import OpenSSH PEM public key string into MS RSACryptoServiceProvider.
    /// </summary>
    /// <param name="pem"></param>
    /// <returns></returns>
    public static RSACryptoServiceProvider ImportPublicKey(string pem)
    {
        var pr = new PemReader(reader: new StringReader(s: pem));
        var publicKey = (AsymmetricKeyParameter)pr.ReadObject();
        var rsaParams = DotNetUtilities.ToRSAParameters(rsaKey: (RsaKeyParameters)publicKey);

        var csp = new RSACryptoServiceProvider(); // cspParams);
        csp.ImportParameters(parameters: rsaParams);
        return csp;
    }

    /// <summary>
    ///     Export private (including public) key from MS RSACryptoServiceProvider into OpenSSH PEM string.
    ///     slightly modified from https://stackoverflow.com/a/23739932/2860309
    /// </summary>
    /// <param name="csp"></param>
    /// <returns></returns>
    public static string ExportPrivateKey(RSACryptoServiceProvider csp, bool armor = true, bool base64Encode = true)
    {
        if (csp is null)
        {
            throw new ArgumentNullException(paramName: nameof(csp));
        }

        if (csp.PublicOnly)
        {
            throw new ArgumentException(
                message: "CSP does not contain a private key",
                paramName: nameof(csp));
        }

        string result; // filled at end of using
        using (var outputStream = new StringWriter())
        {
            var parameters = csp.ExportParameters(includePrivateParameters: true);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(output: stream))
            {
                writer.Write(value: (byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                using (var innerWriter = new BinaryWriter(output: innerStream))
                {
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: new byte[] {0x00}); // Version
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.Modulus);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.Exponent);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.D);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.P);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.Q);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.DP);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.DQ);
                    EncodeIntegerBigEndian(stream: innerWriter,
                        value: parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(stream: writer,
                        length: length);
                    writer.Write(buffer: innerStream.GetBuffer(),
                        index: 0,
                        count: length); // TODO: verify if these should be ToArray()
                }

                // WriteLine terminates with \r\n, we want only \n
                if (armor)
                {
                    outputStream.Write(value: "-----BEGIN RSA PRIVATE KEY-----\n");
                }

                // Output as Base64 with lines chopped at 64 characters
                if (base64Encode)
                {
                    var base64 = Convert.ToBase64String(
                        inArray: stream.GetBuffer(),
                        offset: 0,
                        length: (int)stream.Length).ToCharArray();
                    for (var i = 0; i < base64.Length; i += 64)
                    {
                        outputStream.Write(buffer: base64,
                            index: i,
                            count: Math.Min(val1: 64,
                                val2: base64.Length - i));
                        outputStream.Write(value: "\n");
                    }
                }
                else
                {
                    outputStream.Write(value: stream.GetBuffer());
                }

                if (armor)
                {
                    outputStream.Write(value: "-----END RSA PRIVATE KEY-----");
                }

                result = outputStream.ToString();

                return result;
            } // end using
        } // end using
    } // end func

    /// <summary>
    ///     Export public key from MS RSACryptoServiceProvider into OpenSSH PEM string
    ///     slightly modified from https://stackoverflow.com/a/28407693.
    /// </summary>
    /// <param name="csp"></param>
    /// <param name="armor"></param>
    /// <param name="base64Encode"></param>
    /// <returns></returns>
    public static string ExportPublicKey(RSACryptoServiceProvider csp, bool armor = true, bool base64Encode = true)
    {
        if (csp is null)
        {
            throw new ArgumentNullException(paramName: nameof(csp));
        }

        string result; // filled at end
        using (var outputStream = new StringWriter())
        {
            var parameters = csp.ExportParameters(includePrivateParameters: false);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(output: stream))
            {
                writer.Write(value: (byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                using (var innerWriter = new BinaryWriter(output: innerStream))
                {
                    innerWriter.Write(value: (byte)0x30); // SEQUENCE
                    EncodeLength(stream: innerWriter,
                        length: 13);
                    innerWriter.Write(value: (byte)0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte[] {0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01};
                    EncodeLength(stream: innerWriter,
                        length: rsaEncryptionOid.Length);
                    innerWriter.Write(buffer: rsaEncryptionOid);
                    innerWriter.Write(value: (byte)0x05); // NULL
                    EncodeLength(stream: innerWriter,
                        length: 0);
                    innerWriter.Write(value: (byte)0x03); // BIT STRING
                    using (var bitStringStream = new MemoryStream())
                    using (var bitStringWriter = new BinaryWriter(output: bitStringStream))
                    {
                        bitStringWriter.Write(value: (byte)0x00); // # of unused bits
                        bitStringWriter.Write(value: (byte)0x30); // SEQUENCE
                        using (var paramsStream = new MemoryStream())
                        using (var paramsWriter = new BinaryWriter(output: paramsStream))
                        {
                            EncodeIntegerBigEndian(stream: paramsWriter,
                                value: parameters.Modulus); // Modulus
                            EncodeIntegerBigEndian(stream: paramsWriter,
                                value: parameters.Exponent); // Exponent
                            var paramsLength = (int)paramsStream.Length;
                            EncodeLength(stream: bitStringWriter,
                                length: paramsLength);
                            bitStringWriter.Write(buffer: paramsStream.GetBuffer(),
                                index: 0,
                                count: paramsLength);
                        }

                        var bitStringLength = (int)bitStringStream.Length;
                        EncodeLength(stream: innerWriter,
                            length: bitStringLength);
                        innerWriter.Write(buffer: bitStringStream.GetBuffer(),
                            index: 0,
                            count: bitStringLength);
                    }

                    var length = (int)innerStream.Length;
                    EncodeLength(stream: writer,
                        length: length);
                    writer.Write(buffer: innerStream.GetBuffer(),
                        index: 0,
                        count: length);
                }

                // WriteLine terminates with \r\n, we want only \n
                if (armor)
                {
                    outputStream.Write(value: "-----BEGIN PUBLIC KEY-----\n");
                }

                if (base64Encode)
                {
                    var base64 = Convert.ToBase64String(
                        inArray: stream.GetBuffer(),
                        offset: 0,
                        length: (int)stream.Length).ToCharArray();

                    for (var i = 0; i < base64.Length; i += 64)
                    {
                        outputStream.Write(buffer: base64,
                            index: i,
                            count: Math.Min(val1: 64,
                                val2: base64.Length - i));
                        outputStream.Write(value: "\n");
                    }
                }
                else
                {
                    outputStream.Write(value: stream.GetBuffer());
                }

                if (armor)
                {
                    outputStream.Write(value: "-----END PUBLIC KEY-----");
                }
            }

            result = outputStream.ToString();
        }

        return result;
    }

    /// <summary>
    ///     https://stackoverflow.com/a/23739932/2860309.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="length"></param>
    private static void EncodeLength(BinaryWriter stream, int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(paramName: "length",
                message: "Length must be non-negative");
        }

        if (length < 0x80)
        {
            // Short form
            stream.Write(value: (byte)length);
        }
        else
        {
            // Long form
            var temp = length;
            var bytesRequired = 0;
            while (temp > 0)
            {
                temp >>= 8;
                bytesRequired++;
            }

            stream.Write(value: (byte)(bytesRequired | 0x80));
            for (var i = bytesRequired - 1; i >= 0; i--)
            {
                stream.Write(value: (byte)((length >> (8 * i)) & 0xff));
            }
        }
    }

    /// <summary>
    ///     https://stackoverflow.com/a/23739932/2860309
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="forceUnsigned"></param>
    private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
    {
        stream.Write(value: (byte)0x02); // INTEGER
        var prefixZeros = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != 0)
            {
                break;
            }

            prefixZeros++;
        }

        if (value.Length - prefixZeros == 0)
        {
            EncodeLength(stream: stream,
                length: 1);
            stream.Write(value: (byte)0);
        }
        else
        {
            if (forceUnsigned && value[prefixZeros] > 0x7f)
            {
                // Add a prefix zero to force unsigned if the MSB is 1
                EncodeLength(stream: stream,
                    length: value.Length - prefixZeros + 1);
                stream.Write(value: (byte)0);
            }
            else
            {
                EncodeLength(stream: stream,
                    length: value.Length - prefixZeros);
            }

            for (var i = prefixZeros; i < value.Length; i++)
            {
                stream.Write(value: value[i]);
            }
        }
    }
}
