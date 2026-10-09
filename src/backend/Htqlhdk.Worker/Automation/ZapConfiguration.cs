using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Htqlhdk.Worker.Automation;

public static class ZapConfiguration
{
    private static readonly IReadOnlyDictionary<string, string> EnvironmentMappings =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["HTQLHDK_ZAP_ENABLED"] = "Enabled",
            ["HTQLHDK_ZAP_IMAGE"] = "ImageReference",
            ["HTQLHDK_ZAP_DOCKER_NETWORK"] = "DockerNetwork",
            ["HTQLHDK_ZAP_WORK_ROOT"] = "WorkRoot",
            ["HTQLHDK_ZAP_ARTIFACT_ROOT"] = "ArtifactRoot",
            ["HTQLHDK_ZAP_ARTIFACT_KEY_REF"] = "ArtifactKeyReference"
        };

    public static void ApplyEnvironmentOverrides(IConfigurationManager configuration)
    {
        Dictionary<string, string?> overrides = new(StringComparer.Ordinal);
        foreach ((string environmentName, string optionName) in EnvironmentMappings)
        {
            string? value = Environment.GetEnvironmentVariable(environmentName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                overrides[$"{ZapAutomationOptions.SectionName}:{optionName}"] = value;
            }
        }

        configuration.AddInMemoryCollection(overrides);
    }

    public static string ResolveDatabaseConnectionString(string preferredEnvironmentVariable)
    {
        string? direct = Environment.GetEnvironmentVariable(preferredEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        string? host = Environment.GetEnvironmentVariable("HTQLHDK_DB_HOST");
        string? database = Environment.GetEnvironmentVariable("HTQLHDK_DB_NAME");
        string? username = Environment.GetEnvironmentVariable("HTQLHDK_DB_APP_USER");
        string? password = Environment.GetEnvironmentVariable("HTQLHDK_DB_APP_PASSWORD");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"ZAP automation requires {preferredEnvironmentVariable} or the HTQLHDK_DB_* application settings.");
        }

        int port = 5432;
        string? portValue = Environment.GetEnvironmentVariable("HTQLHDK_DB_PORT");
        if (!string.IsNullOrWhiteSpace(portValue) && !int.TryParse(portValue, out port))
        {
            throw new InvalidOperationException("HTQLHDK_DB_PORT is invalid.");
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            Pooling = true,
            ApplicationName = "HTQLHDK ZAP Worker",
            Timezone = "UTC"
        };
        return builder.ConnectionString;
    }
}
