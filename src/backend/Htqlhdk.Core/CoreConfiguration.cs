using Npgsql;

namespace Htqlhdk.Core;

public static class CoreConfiguration
{
    public static string Root()
    {
        string? configured = Environment.GetEnvironmentVariable("HTQLHDK_APP_ROOT");
        if (!string.IsNullOrEmpty(configured)) { return Path.GetFullPath(configured); }
        DirectoryInfo? directory = new(Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HTQLHDK.slnx"))) { return directory.FullName; }
            directory = directory.Parent;
        }
        return Environment.CurrentDirectory;
    }
    public static void LoadLocalEnvironment(bool development)
    {
        if (!development) { return; }
        string file = Path.Combine(Root(), ".env.local");
        if (!File.Exists(file)) { return; }
        foreach (string line in File.ReadLines(file))
        {
            int separator = line.IndexOf('=');
            if (separator < 1 || !line.StartsWith("HTQLHDK_", StringComparison.Ordinal)) { continue; }
            string name = line[..separator];
            if (Environment.GetEnvironmentVariable(name) is null) { Environment.SetEnvironmentVariable(name, line[(separator + 1)..]); }
        }
    }
    public static string ConnectionString()
    {
        string? direct = Environment.GetEnvironmentVariable("HTQLHDK_DATABASE_URL");
        NpgsqlConnectionStringBuilder settings = !string.IsNullOrEmpty(direct) ? new(direct) : new()
        {
            Host = Environment.GetEnvironmentVariable("HTQLHDK_DB_HOST") ?? "127.0.0.1",
            Port = int.Parse(Environment.GetEnvironmentVariable("HTQLHDK_DB_PORT") ?? "54329", System.Globalization.CultureInfo.InvariantCulture),
            Database = Environment.GetEnvironmentVariable("HTQLHDK_DB_NAME") ?? throw new InvalidOperationException("Database configuration is required."),
            Username = Environment.GetEnvironmentVariable("HTQLHDK_DB_APP_USER") ?? throw new InvalidOperationException("Database account configuration is required."),
            Password = Environment.GetEnvironmentVariable("HTQLHDK_DB_APP_PASSWORD") ?? throw new InvalidOperationException("Database credential configuration is required."),
            Timezone = "UTC", ApplicationName = "HTQLHDK Core13", IncludeErrorDetail = false
        };
        if (Environment.GetEnvironmentVariable("HTQLHDK_ENVIRONMENT") == "Local")
        {
            Fields.Require(settings.Database?.EndsWith("_local", StringComparison.Ordinal) == true && settings.Host is "127.0.0.1" or "localhost" or "::1", "Môi trường Local chỉ sử dụng database cục bộ có tên kết thúc bằng _local.");
        }
        return settings.ConnectionString;
    }
    public static Protection Storage(bool development)
    {
        string keyFile = Environment.GetEnvironmentVariable("HTQLHDK_ENCRYPTION_KEY_FILE") ?? Path.Combine(Root(), "local-data", "keys", "core13.key");
        string storageRoot = Environment.GetEnvironmentVariable("HTQLHDK_ARTIFACT_ROOT") ?? Path.Combine(Root(), "local-data", "core13", "artifacts");
        if (!File.Exists(keyFile))
        {
            if (!development) { throw new InvalidOperationException("A provisioned encryption key is required."); }
            Directory.CreateDirectory(Path.GetDirectoryName(keyFile)!);
            using FileStream file = new(keyFile, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            file.Write(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }
        return new(keyFile, storageRoot);
    }
}
