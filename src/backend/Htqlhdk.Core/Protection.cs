using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Htqlhdk.Core;

public sealed partial class Protection
{
    private readonly byte[] key;
    private readonly string root;
    public Protection(string keyFile, string storageRoot)
    {
        key = File.ReadAllBytes(keyFile);
        Fields.Require(key.Length == 32, "Khóa mã hóa phải có 32 byte.");
        root = Path.GetFullPath(storageRoot);
        Directory.CreateDirectory(root);
    }
    public byte[] Encrypt(ReadOnlySpan<byte> content)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] encrypted = new byte[content.Length];
        byte[] tag = new byte[16];
        using AesGcm aes = new(key, 16);
        aes.Encrypt(nonce, content, encrypted, tag);
        return [.. nonce, .. tag, .. encrypted];
    }
    public byte[] Decrypt(ReadOnlySpan<byte> content)
    {
        Fields.Require(content.Length >= 28, "Tệp mã hóa không hợp lệ.");
        byte[] result = new byte[content.Length - 28];
        using AesGcm aes = new(key, 16);
        aes.Decrypt(content[..12], content[28..], content[12..28], result);
        return result;
    }
    public string Fingerprint(string identity) => Convert.ToHexStringLower(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes("fingerprint:v1:" + identity)));
    public async Task<string> StoreAsync(byte[] content, CancellationToken ct)
    {
        string name = Guid.NewGuid().ToString("N") + ".enc";
        await File.WriteAllBytesAsync(Path.Combine(root, name), Encrypt(content), ct);
        return name;
    }
    public async Task<byte[]> ReadAsync(string name, CancellationToken ct)
    {
        Fields.Require(Path.GetFileName(name) == name && name.EndsWith(".enc", StringComparison.Ordinal), "Tệp không hợp lệ.");
        return Decrypt(await File.ReadAllBytesAsync(Path.Combine(root, name), ct));
    }
    public void Remove(string name)
    {
        if (Path.GetFileName(name) == name) { File.Delete(Path.Combine(root, name)); }
    }
    public static string Url(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)) { return "[URL đã che]"; }
        return uri.GetLeftPart(UriPartial.Path) + (uri.Query.Length > 0 ? "?[đã che]" : "");
    }
    public static string Mask(string value)
    {
        string masked = UrlPattern().Replace(value, match => Url(match.Value));
        return SecretPattern().Replace(masked, "$1=[đã che]");
    }
    [GeneratedRegex(@"https?://[^\s<>""']+", RegexOptions.CultureInvariant)]
    private static partial Regex UrlPattern();
    [GeneratedRegex(@"(?i)(password|passwd|token|secret|authorization|api[_-]?key|bearer)\s*[:= ]\s*[^\s,;]+", RegexOptions.CultureInvariant)]
    private static partial Regex SecretPattern();
}
