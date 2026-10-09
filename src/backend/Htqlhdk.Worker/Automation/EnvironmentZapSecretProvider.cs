using System.Text.RegularExpressions;

namespace Htqlhdk.Worker.Automation;

public interface IZapSecretProvider
{
    ValueTask<string> GetSecretAsync(string reference, CancellationToken cancellationToken);
}

public sealed class EnvironmentZapSecretProvider : IZapSecretProvider
{
    private static readonly Regex AllowedReference = new(
        "^env:(HTQLHDK_ZAP_[A-Z0-9_]+)$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public ValueTask<string> GetSecretAsync(string reference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Match match = AllowedReference.Match(reference);
        if (!match.Success)
        {
            throw new InvalidOperationException("Unsupported ZAP secret reference.");
        }

        string variableName = match.Groups[1].Value;
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required ZAP secret {variableName} is unavailable.");
        }

        if (value.Contains('\r') || value.Contains('\n'))
        {
            throw new InvalidOperationException($"Required ZAP secret {variableName} is not a valid header value.");
        }

        return ValueTask.FromResult(value);
    }
}
