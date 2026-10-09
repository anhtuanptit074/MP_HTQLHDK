using System.Data;
using System.Globalization;
using System.Text.Json.Nodes;
using Npgsql;
using NpgsqlTypes;

namespace Htqlhdk.Core;

public sealed class BusinessException(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}

public sealed class CoreDatabase(NpgsqlDataSource source)
{
    public async Task<DbSession> OpenAsync(CancellationToken ct = default)
    {
        NpgsqlConnection connection = await source.OpenConnectionAsync(ct);
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        DbSession session = new(connection, transaction);
        await session.ExecuteAsync("SET LOCAL search_path=htqlhdk,public", ct);
        return session;
    }
}

public sealed class DbSession(NpgsqlConnection connection, NpgsqlTransaction transaction) : IAsyncDisposable
{
    public async Task<List<JsonObject>> QueryAsync(string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        await using NpgsqlCommand command = Command(sql, parameters);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        List<JsonObject> rows = [];
        while (await reader.ReadAsync(ct))
        {
            JsonObject row = [];
            for (int index = 0; index < reader.FieldCount; index++)
            {
                string key = Camel(reader.GetName(index));
                if (reader.IsDBNull(index)) { row[key] = null; continue; }
                object value = reader.GetValue(index);
                row[key] = value switch
                {
                    DateTime timestamp => JsonValue.Create(timestamp.ToUniversalTime()),
                    TimeSpan time => JsonValue.Create(time.ToString(@"hh\:mm", CultureInfo.InvariantCulture)),
                    byte[] bytes => JsonValue.Create(Convert.ToBase64String(bytes)),
                    _ when reader.GetDataTypeName(index) == "jsonb" => JsonNode.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!),
                    _ => System.Text.Json.JsonSerializer.SerializeToNode(value)
                };
            }
            rows.Add(row);
        }
        return rows;
    }

    public async Task<JsonObject> OneAsync(string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        List<JsonObject> rows = await QueryAsync(sql, ct, parameters);
        return rows.FirstOrDefault() ?? throw new BusinessException(404, "NOT_FOUND", "Không tìm thấy trong phạm vi được phép.");
    }

    public async Task<int> ExecuteAsync(string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        await using NpgsqlCommand command = Command(sql, parameters);
        return await command.ExecuteNonQueryAsync(ct);
    }

    private NpgsqlCommand Command(string sql, (string Name, object? Value)[] parameters)
    {
        NpgsqlCommand command = new(sql, connection, transaction) { CommandTimeout = 30 };
        foreach ((string name, object? value) in parameters)
        {
            if (value is null) { command.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Unknown) { Value = DBNull.Value }); }
            else { command.Parameters.AddWithValue(name, value); }
        }
        return command;
    }

    public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);
    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
        await connection.DisposeAsync();
    }
    private static string Camel(string name)
    {
        string[] parts = name.Split('_');
        return parts[0] + string.Concat(parts.Skip(1).Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}

public static class Fields
{
    public static string Text(this JsonObject value, string name, string? fallback = null)
    {
        string? text = value[name]?.GetValue<string>() ?? fallback;
        if (string.IsNullOrWhiteSpace(text) || text.Length > 8000)
            throw new BusinessException(422, "VALIDATION", $"Trường {name} không hợp lệ.");
        return text.Trim();
    }
    public static string? Optional(this JsonObject value, string name) => value[name]?.GetValue<string>()?.Trim();
    public static long Number(this JsonObject value, string name) => value.NullableNumber(name) ?? throw new BusinessException(422, "VALIDATION", $"Thiếu {name}.");
    public static long? NullableNumber(this JsonObject value, string name)
    {
        if (value[name] is null) { return null; }
        return long.TryParse(value[name]!.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long result) ? result : throw new BusinessException(422, "VALIDATION", $"Trường {name} phải là số nguyên.");
    }
    public static bool Flag(this JsonObject value, string name, bool fallback = false) => value[name]?.GetValue<bool>() ?? fallback;
    public static DateTime Date(this JsonObject value, string name) => System.Text.Json.JsonSerializer.Deserialize<DateTimeOffset>(value[name]?.ToJsonString() ?? throw new BusinessException(422, "VALIDATION", $"Thiếu {name}.")).UtcDateTime;
    public static DateTime? NullableDate(this JsonObject value, string name) => value[name] is null ? null : value.Date(name);
    public static JsonArray Array(this JsonObject value, string name) => value[name]?.AsArray() ?? throw new BusinessException(422, "VALIDATION", $"Thiếu {name}.");
    public static void Require(bool valid, string message, int status = 422, string code = "BUSINESS_RULE")
    {
        if (!valid) { throw new BusinessException(status, code, message); }
    }
}
