using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Htqlhdk.Core;

public static class Credentials
{
    private static readonly PasswordHasher<string> Hasher = new(new Microsoft.Extensions.Options.OptionsWrapper<PasswordHasherOptions>(new() { IterationCount = 210000 }));
    public static string Hash(string password)
    {
        Fields.Require(password.Length is >= 12 and <= 256, "Mật khẩu cần từ 12 đến 256 ký tự.");
        return Hasher.HashPassword("user", password);
    }
    public static bool Verify(string hash, string password) => Hasher.VerifyHashedPassword("user", hash, password) != PasswordVerificationResult.Failed;
    public static string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(12)) + "aA!";
}
