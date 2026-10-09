using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Htqlhdk.Worker.Automation;

public interface IZapArtifactStore
{
    Task<StoredZapArtifact> StoreAsync(
        string executionCode,
        string sourcePath,
        string artifactKind,
        string mediaType,
        CancellationToken cancellationToken);
}

public sealed class LocalZapArtifactStore(IOptions<ZapAutomationOptions> options) : IZapArtifactStore
{
    private readonly ZapAutomationOptions options = options.Value;

    public async Task<StoredZapArtifact> StoreAsync(
        string executionCode,
        string sourcePath,
        string artifactKind,
        string mediaType,
        CancellationToken cancellationToken)
    {
        string root = Path.GetFullPath(options.ArtifactRoot);
        string extension = mediaType == "application/json" ? ".json" : ".html";
        string safeKind = mediaType == "application/json" ? "traditional-json" : "traditional-html";

        await using FileStream source = new(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        string sha256 = Convert.ToHexString(await SHA256.HashDataAsync(source, cancellationToken))
            .ToLowerInvariant();
        source.Position = 0;

        string relativeKey = Path.Combine(executionCode, $"{safeKind}-{sha256}{extension}");
        string destination = Path.GetFullPath(Path.Combine(root, relativeKey));
        if (!destination.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Artifact destination escaped the configured root.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (!File.Exists(destination))
        {
            await using FileStream output = new(
                destination,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.Asynchronous);
            await source.CopyToAsync(output, cancellationToken);
            await output.FlushAsync(cancellationToken);
        }
        else
        {
            await using FileStream existing = new(
                destination,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            string existingHash = Convert.ToHexString(await SHA256.HashDataAsync(existing, cancellationToken))
                .ToLowerInvariant();
            if (!string.Equals(existingHash, sha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException("An existing artifact does not match its content-addressed key.");
            }
        }

        FileInfo stored = new(destination);
        return new StoredZapArtifact(
            relativeKey.Replace(Path.DirectorySeparatorChar, '/'),
            sha256,
            stored.Length,
            mediaType,
            artifactKind,
            Path.GetFileName(sourcePath));
    }
}
