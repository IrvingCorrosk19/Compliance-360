using System.Security.Cryptography;
using System.Text;
using Compliance360.Application.Mfa;

namespace Compliance360.Infrastructure.Mfa;

public sealed class Base64MfaSecretProtector : IMfaSecretProtector
{
    public string Protect(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(secret));
    }

    public string Unprotect(string encryptedSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedSecret);
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedSecret));
    }
}

public sealed class TotpService : ITotpService
{
    private const int TimeStepSeconds = 30;
    private const int CodeDigits = 6;

    public string GenerateSecret()
    {
        return Base32Encode(RandomNumberGenerator.GetBytes(20));
    }

    public string GenerateCode(string secret, DateTimeOffset timestampUtc)
    {
        var counter = timestampUtc.ToUnixTimeSeconds() / TimeStepSeconds;
        return GenerateCodeForCounter(secret, counter);
    }

    public bool VerifyCode(string secret, string code, DateTimeOffset timestampUtc, int allowedDriftSteps = 1)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var normalizedCode = code.Trim();
        var counter = timestampUtc.ToUnixTimeSeconds() / TimeStepSeconds;
        for (var offset = -allowedDriftSteps; offset <= allowedDriftSteps; offset++)
        {
            if (GenerateCodeForCounter(secret, counter + offset) == normalizedCode)
            {
                return true;
            }
        }

        return false;
    }

    private static string GenerateCodeForCounter(string secret, long counter)
    {
        var key = Base32Decode(secret);
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24)
            | ((hash[offset + 1] & 0xFF) << 16)
            | ((hash[offset + 2] & 0xFF) << 8)
            | (hash[offset + 3] & 0xFF);
        var otp = binary % (int)Math.Pow(10, CodeDigits);
        return otp.ToString($"D{CodeDigits}");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var value in data)
        {
            buffer = (buffer << 8) | value;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                output.Append(alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            output.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return output.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var normalized = input.Trim().Replace("=", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var output = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var character in normalized)
        {
            var value = alphabet.IndexOf(character, StringComparison.Ordinal);
            if (value < 0)
            {
                throw new ArgumentException("Invalid Base32 secret.", nameof(input));
            }

            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 255));
                bitsLeft -= 8;
            }
        }

        return output.ToArray();
    }
}
