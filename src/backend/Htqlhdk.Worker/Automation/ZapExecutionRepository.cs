using System.Data;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace Htqlhdk.Worker.Automation;

public sealed class ZapExecutionRepository(NpgsqlDataSource dataSource, TimeProvider timeProvider)
{
    public async Task<int> RecoverExpiredLeasesAsync(int batchSize, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await SetSearchPathAsync(connection, transaction, cancellationToken);

        const string selectSql = """
            SELECT e.id, e.scan_id, s.asset_id
            FROM zap_execution e
            JOIN scan_occurrence s ON s.id = e.scan_id
            WHERE e.status NOT IN ('Report Received','Failed','Timed Out','Cancelled')
              AND e.lease_expires_at IS NOT NULL
              AND e.lease_expires_at < @now
            ORDER BY e.lease_expires_at, e.id
            FOR UPDATE OF e, s SKIP LOCKED
            LIMIT @batch_size
            """;
        List<ExpiredExecution> expired = [];
        await using (NpgsqlCommand select = new(selectSql, connection, transaction))
        {
            select.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            select.Parameters.AddWithValue("batch_size", NpgsqlDbType.Integer, batchSize);
            await using NpgsqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                expired.Add(new ExpiredExecution(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2)));
            }
        }

        foreach (ExpiredExecution item in expired)
        {
            const string executionSql = """
                UPDATE zap_execution
                SET status = 'Failed', finished_at = @now, heartbeat_at = @now,
                    lease_owner = NULL, lease_expires_at = NULL,
                    failure_code = 'WORKER_LEASE_EXPIRED',
                    failure_message_masked = 'Worker heartbeat expired; scan was not retried automatically.'
                WHERE id = @id
                """;
            await using (NpgsqlCommand execution = new(executionSql, connection, transaction))
            {
                execution.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
                execution.Parameters.AddWithValue("id", item.ExecutionId);
                await execution.ExecuteNonQueryAsync(cancellationToken);
            }

            const string scanSql = """
                UPDATE scan_occurrence
                SET status = 'Failed', outcome_reason = 'Worker heartbeat expired; manual review is required.',
                    ended_at = CASE WHEN started_at IS NULL THEN NULL ELSE @now END
                WHERE id = @scan_id AND status = 'Planned'
                """;
            await using (NpgsqlCommand scan = new(scanSql, connection, transaction))
            {
                scan.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
                scan.Parameters.AddWithValue("scan_id", item.ScanId);
                await scan.ExecuteNonQueryAsync(cancellationToken);
            }

            await InsertAuditAsync(
                connection,
                transaction,
                item.AssetId,
                item.ExecutionId,
                "zap.execution.lease_expired",
                "Worker heartbeat expired; scan was not retried automatically.",
                cancellationToken,
                "Failure");
        }

        await transaction.CommitAsync(cancellationToken);
        return expired.Count;
    }

    public async Task<int> DispatchDueSchedulesAsync(
        int batchSize,
        string imageReference,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await SetSearchPathAsync(connection, transaction, cancellationToken);

        List<DueSchedule> schedules = [];
        const string selectSql = """
            SELECT s.id, s.asset_id, s.scope_id, s.next_due_at, s.planned_duration_minutes,
                   s.operations_team_id, s.target_urls, s.frequency, s.recurrence_rule,
                   s.timezone, s.effective_until, p.id, p.service_user_id, p.code, p.version_no
            FROM scan_schedule s
            JOIN asset a ON a.id = s.asset_id AND a.status = 'Active'
            JOIN asset_scope_revision scope
              ON scope.id = s.scope_id AND scope.asset_id = s.asset_id AND scope.status = 'Approved'
            JOIN zap_scan_profile p ON p.id = s.zap_profile_id AND p.status = 'Active'
            WHERE s.status = 'Active'
              AND s.automation_enabled
              AND s.next_due_at <= @now
              AND (s.effective_until IS NULL OR s.next_due_at < s.effective_until)
            ORDER BY s.next_due_at, s.id
            FOR UPDATE OF s SKIP LOCKED
            LIMIT @batch_size
            """;
        await using (NpgsqlCommand command = new(selectSql, connection, transaction))
        {
            command.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            command.Parameters.AddWithValue("batch_size", NpgsqlDbType.Integer, batchSize);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                schedules.Add(new DueSchedule(
                    reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetFieldValue<DateTimeOffset>(3),
                    reader.GetInt32(4), reader.GetInt64(5), reader.GetFieldValue<string[]>(6), reader.GetString(7),
                    reader.GetString(8), reader.GetString(9), reader.IsDBNull(10) ? null : reader.GetFieldValue<DateTimeOffset>(10),
                    reader.GetInt64(11), reader.GetInt64(12), reader.GetString(13), reader.GetInt32(14)));
            }
        }

        foreach (DueSchedule schedule in schedules)
        {
            DateTimeOffset nextDue = ScheduleRecurrence.CalculateNext(
                schedule.DueAt,
                schedule.Frequency,
                schedule.RecurrenceRule,
                schedule.Timezone);
            string scanCode = NewCode("ZAS");
            string executionCode = NewCode("ZAE");
            Guid correlationId = Guid.NewGuid();
            string snapshot = JsonSerializer.Serialize(new
            {
                source = "ZAP Automation Framework",
                scheduleId = schedule.Id,
                scopeId = schedule.ScopeId,
                profileCode = schedule.ProfileCode,
                profileVersion = schedule.ProfileVersion,
                targetCount = schedule.TargetUrls.Length,
                imageReference
            });

            const string insertSql = """
                WITH created_scan AS (
                  INSERT INTO scan_occurrence
                    (code, asset_id, scope_id, schedule_id, purpose, planned_at, planned_end_at,
                     operations_team_id, status, config_snapshot, target_urls)
                  VALUES
                    (@scan_code, @asset_id, @scope_id, @schedule_id, 'Periodic', @planned_at,
                     @planned_at + make_interval(mins => @duration), @team_id, 'Planned', @snapshot, @targets)
                  ON CONFLICT (schedule_id, planned_at) WHERE schedule_id IS NOT NULL DO NOTHING
                  RETURNING id
                ), created_execution AS (
                  INSERT INTO zap_execution
                    (code, scan_id, profile_id, service_user_id, image_reference, configuration_snapshot)
                  SELECT @execution_code, id, @profile_id, @service_user_id, @image_reference, @snapshot::jsonb
                  FROM created_scan
                  RETURNING id, scan_id
                )
                INSERT INTO audit_log
                  (actor_kind, category, action, asset_id, object_type, object_id, outcome,
                   reason_masked, correlation_id)
                SELECT 'System', 'System', 'zap.execution.queued', @asset_id, 'zap_execution',
                       id::text, 'Success', 'Automated approved schedule became due', @correlation_id
                FROM created_execution
                """;
            await using (NpgsqlCommand insert = new(insertSql, connection, transaction))
            {
                insert.Parameters.AddWithValue("scan_code", scanCode);
                insert.Parameters.AddWithValue("execution_code", executionCode);
                insert.Parameters.AddWithValue("asset_id", schedule.AssetId);
                insert.Parameters.AddWithValue("scope_id", schedule.ScopeId);
                insert.Parameters.AddWithValue("schedule_id", schedule.Id);
                insert.Parameters.AddWithValue("planned_at", NpgsqlDbType.TimestampTz, schedule.DueAt);
                insert.Parameters.AddWithValue("duration", schedule.PlannedDurationMinutes);
                insert.Parameters.AddWithValue("team_id", schedule.OperationsTeamId);
                insert.Parameters.AddWithValue("snapshot", snapshot);
                insert.Parameters.AddWithValue("targets", schedule.TargetUrls);
                insert.Parameters.AddWithValue("profile_id", schedule.ProfileId);
                insert.Parameters.AddWithValue("service_user_id", schedule.ServiceUserId);
                insert.Parameters.AddWithValue("image_reference", imageReference);
                insert.Parameters.AddWithValue("correlation_id", correlationId);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            const string updateScheduleSql = """
                UPDATE scan_schedule
                SET next_due_at = @next_due,
                    status = CASE
                      WHEN effective_until IS NOT NULL AND @next_due >= effective_until THEN 'Expired'::schedule_status
                      ELSE status
                    END
                WHERE id = @schedule_id
                """;
            await using NpgsqlCommand update = new(updateScheduleSql, connection, transaction);
            update.Parameters.AddWithValue("next_due", NpgsqlDbType.TimestampTz, nextDue);
            update.Parameters.AddWithValue("schedule_id", schedule.Id);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return schedules.Count;
    }

    public async Task<ZapJobSpecification?> ClaimNextAsync(
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await SetSearchPathAsync(connection, transaction, cancellationToken);

        const string claimSql = """
            WITH candidate AS (
              SELECT id
              FROM zap_execution
              WHERE status = 'Queued' AND attempt_count < max_attempts
              ORDER BY requested_at, id
              FOR UPDATE SKIP LOCKED
              LIMIT 1
            )
            UPDATE zap_execution e
            SET status = 'Preparing Plan', attempt_count = attempt_count + 1,
                started_at = COALESCE(started_at, @now), heartbeat_at = @now,
                lease_owner = @worker_id, lease_expires_at = @lease_expires,
                runner_name = @worker_id
            FROM candidate
            WHERE e.id = candidate.id
            RETURNING e.id
            """;
        long? executionId;
        await using (NpgsqlCommand claim = new(claimSql, connection, transaction))
        {
            claim.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            claim.Parameters.AddWithValue("worker_id", workerId);
            claim.Parameters.AddWithValue("lease_expires", NpgsqlDbType.TimestampTz, now.Add(leaseDuration));
            object? claimed = await claim.ExecuteScalarAsync(cancellationToken);
            executionId = claimed is null or DBNull ? null : Convert.ToInt64(claimed, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (executionId is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        const string jobSql = """
            SELECT e.id, e.code, e.scan_id, s.asset_id, e.service_user_id, s.target_urls,
                   p.spider_kind::text, p.active_scan_enabled, p.auth_kind::text,
                   p.auth_header_name, p.auth_secret_ref, p.spider_max_minutes,
                   p.passive_wait_max_minutes, p.active_scan_max_minutes, p.max_rule_minutes,
                   p.request_delay_ms, p.thread_per_host, e.started_at,
                   EXTRACT(EPOCH FROM (s.planned_end_at - s.planned_at))::integer / 60
            FROM zap_execution e
            JOIN scan_occurrence s ON s.id = e.scan_id
            JOIN zap_scan_profile p ON p.id = e.profile_id
            WHERE e.id = @execution_id AND e.lease_owner = @worker_id
            """;
        JobRow row;
        await using (NpgsqlCommand command = new(jobSql, connection, transaction))
        {
            command.Parameters.AddWithValue("execution_id", executionId.Value);
            command.Parameters.AddWithValue("worker_id", workerId);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("Claimed ZAP execution disappeared.");
            }

            row = new JobRow(
                reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt64(3), reader.GetInt64(4),
                reader.GetFieldValue<string[]>(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9), reader.IsDBNull(10) ? null : reader.GetString(10),
                reader.GetInt32(11), reader.GetInt32(12), reader.GetInt32(13), reader.GetInt32(14),
                reader.GetInt32(15), reader.GetInt32(16), reader.GetFieldValue<DateTimeOffset>(17), reader.GetInt32(18));
        }

        const string rulesSql = """
            SELECT r.effect, r.target_type, r.target_value, r.match_mode
            FROM scan_occurrence s
            JOIN asset_scope_rule r ON r.scope_id = s.scope_id
            WHERE s.id = @scan_id
            ORDER BY r.effect, r.id
            """;
        List<ZapScopeRule> rules = [];
        await using (NpgsqlCommand command = new(rulesSql, connection, transaction))
        {
            command.Parameters.AddWithValue("scan_id", row.ScanId);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rules.Add(new ZapScopeRule(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return new ZapJobSpecification(
            row.ExecutionId, row.ExecutionCode, row.ScanId, row.AssetId, row.ServiceUserId,
            row.TargetUrls, [.. rules], Enum.Parse<ZapSpiderKind>(row.SpiderKind), row.ActiveScanEnabled,
            Enum.Parse<ZapAuthenticationKind>(row.AuthenticationKind), row.AuthenticationHeaderName,
            row.AuthenticationSecretReference, row.SpiderMaxMinutes, row.PassiveWaitMaxMinutes,
            row.ActiveScanMaxMinutes, row.MaxRuleMinutes, row.RequestDelayMilliseconds,
            row.ThreadPerHost, row.StartedAt, row.PlannedDurationMinutes);
    }

    public async Task<bool> RenewLeaseAsync(
        long executionId,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        const string sql = """
            UPDATE htqlhdk.zap_execution
            SET heartbeat_at = @now, lease_expires_at = @lease_expires
            WHERE id = @id AND lease_owner = @worker_id
              AND status NOT IN ('Report Received','Failed','Timed Out','Cancelled')
            RETURNING cancel_requested_at IS NOT NULL
            """;
        await using NpgsqlCommand command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
        command.Parameters.AddWithValue("lease_expires", NpgsqlDbType.TimestampTz, now.Add(leaseDuration));
        command.Parameters.AddWithValue("id", executionId);
        command.Parameters.AddWithValue("worker_id", workerId);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("ZAP execution lease was lost.");
        }

        return (bool)result;
    }

    public Task SetStatusAsync(long executionId, string workerId, string status, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE htqlhdk.zap_execution SET status = @status::htqlhdk.zap_execution_status
            WHERE id = @id AND lease_owner = @worker_id
            """;
        return ExecuteAsync(sql, executionId, workerId, status, cancellationToken);
    }

    public async Task CompleteAsync(
        ZapJobSpecification job,
        string workerId,
        ZapReportMetadata report,
        StoredZapArtifact jsonArtifact,
        StoredZapArtifact? htmlArtifact,
        int exitCode,
        string artifactKeyReference,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await SetSearchPathAsync(connection, transaction, cancellationToken);

        const string scanSql = """
            UPDATE scan_occurrence
            SET status = 'Completed', started_at = COALESCE(started_at, @started_at), ended_at = @ended_at,
                performed_by = @service_user_id, outcome_reason = NULL
            WHERE id = @scan_id AND status = 'Planned'
            """;
        await using (NpgsqlCommand scan = new(scanSql, connection, transaction))
        {
            scan.Parameters.AddWithValue("started_at", NpgsqlDbType.TimestampTz, job.StartedAt);
            scan.Parameters.AddWithValue("ended_at", NpgsqlDbType.TimestampTz, now);
            scan.Parameters.AddWithValue("service_user_id", job.ServiceUserId);
            scan.Parameters.AddWithValue("scan_id", job.ScanId);
            if (await scan.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("Scan occurrence is no longer eligible for automated completion.");
            }
        }

        string batchCode = NewCode("ZAB");
        const string batchSql = """
            INSERT INTO import_batch
              (code, asset_id, scan_id, file_sha256, zap_version, template_name, imported_by,
               status, total_records, raw_object_key, raw_original_name, raw_size_bytes,
               raw_format, raw_key_ref)
            VALUES
              (@code, @asset_id, @scan_id, @sha256, @zap_version, 'Traditional JSON Report',
               @service_user_id, 'Received', @total_records, @object_key, @original_name,
               @size_bytes, 'JSON', @key_ref)
            ON CONFLICT (asset_id, scan_id, file_sha256) DO UPDATE
              SET file_sha256 = EXCLUDED.file_sha256
            RETURNING id
            """;
        long importBatchId;
        await using (NpgsqlCommand batch = new(batchSql, connection, transaction))
        {
            batch.Parameters.AddWithValue("code", batchCode);
            batch.Parameters.AddWithValue("asset_id", job.AssetId);
            batch.Parameters.AddWithValue("scan_id", job.ScanId);
            batch.Parameters.AddWithValue("sha256", report.Sha256);
            batch.Parameters.AddWithValue("zap_version", report.ZapVersion);
            batch.Parameters.AddWithValue("service_user_id", job.ServiceUserId);
            batch.Parameters.AddWithValue("total_records", report.AlertCount);
            batch.Parameters.AddWithValue("object_key", jsonArtifact.ObjectKey);
            batch.Parameters.AddWithValue("original_name", jsonArtifact.OriginalName);
            batch.Parameters.AddWithValue("size_bytes", jsonArtifact.SizeBytes);
            batch.Parameters.AddWithValue("key_ref", artifactKeyReference);
            importBatchId = Convert.ToInt64(
                await batch.ExecuteScalarAsync(cancellationToken),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        await InsertArtifactAsync(connection, transaction, job.ExecutionId, jsonArtifact, artifactKeyReference, cancellationToken);
        if (htmlArtifact is not null)
        {
            await InsertArtifactAsync(connection, transaction, job.ExecutionId, htmlArtifact, artifactKeyReference, cancellationToken);
        }

        const string executionSql = """
            UPDATE zap_execution
            SET status = 'Report Received', finished_at = @now, heartbeat_at = @now,
                lease_owner = NULL, lease_expires_at = NULL, exit_code = @exit_code,
                zap_version = @zap_version, import_batch_id = @batch_id
            WHERE id = @id AND lease_owner = @worker_id
            """;
        await using (NpgsqlCommand execution = new(executionSql, connection, transaction))
        {
            execution.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            execution.Parameters.AddWithValue("exit_code", exitCode);
            execution.Parameters.AddWithValue("zap_version", report.ZapVersion);
            execution.Parameters.AddWithValue("batch_id", importBatchId);
            execution.Parameters.AddWithValue("id", job.ExecutionId);
            execution.Parameters.AddWithValue("worker_id", workerId);
            if (await execution.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("ZAP execution lease was lost before completion.");
            }
        }

        await InsertAuditAsync(
            connection, transaction, job.AssetId, job.ExecutionId, "zap.execution.report_received",
            "ZAP report stored and queued for import", cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task FailAsync(
        ZapJobSpecification job,
        string workerId,
        string terminalStatus,
        string failureCode,
        string maskedMessage,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await SetSearchPathAsync(connection, transaction, cancellationToken);

        const string executionSql = """
            UPDATE zap_execution
            SET status = @status::zap_execution_status, finished_at = @now, heartbeat_at = @now,
                lease_owner = NULL, lease_expires_at = NULL, failure_code = @failure_code,
                failure_message_masked = @message
            WHERE id = @id AND lease_owner = @worker_id
            """;
        await using (NpgsqlCommand execution = new(executionSql, connection, transaction))
        {
            execution.Parameters.AddWithValue("status", terminalStatus);
            execution.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            execution.Parameters.AddWithValue("failure_code", failureCode);
            execution.Parameters.AddWithValue("message", maskedMessage);
            execution.Parameters.AddWithValue("id", job.ExecutionId);
            execution.Parameters.AddWithValue("worker_id", workerId);
            if (await execution.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("ZAP execution lease was lost before failure could be recorded.");
            }
        }

        const string scanSql = """
            UPDATE scan_occurrence
            SET status = CASE WHEN @status = 'Cancelled' THEN 'Cancelled'::scan_status ELSE 'Failed'::scan_status END,
                outcome_reason = @message, ended_at = CASE WHEN started_at IS NULL THEN NULL ELSE @now END
            WHERE id = @scan_id AND status = 'Planned'
            """;
        await using (NpgsqlCommand scan = new(scanSql, connection, transaction))
        {
            scan.Parameters.AddWithValue("status", terminalStatus);
            scan.Parameters.AddWithValue("message", maskedMessage);
            scan.Parameters.AddWithValue("now", NpgsqlDbType.TimestampTz, now);
            scan.Parameters.AddWithValue("scan_id", job.ScanId);
            await scan.ExecuteNonQueryAsync(cancellationToken);
        }

        await InsertAuditAsync(
            connection, transaction, job.AssetId, job.ExecutionId, "zap.execution.failed", maskedMessage,
            cancellationToken, "Failure");
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ExecuteAsync(
        string sql,
        long executionId,
        string workerId,
        string status,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("id", executionId);
        command.Parameters.AddWithValue("worker_id", workerId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("ZAP execution state update lost its lease.");
        }
    }

    private static async Task InsertArtifactAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long executionId,
        StoredZapArtifact artifact,
        string keyReference,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO zap_artifact
              (execution_id, artifact_kind, object_key, sha256, media_type, size_bytes, key_ref)
            VALUES (@execution_id, @kind, @key, @sha256, @media_type, @size, @key_ref)
            ON CONFLICT (execution_id, artifact_kind) DO NOTHING
            """;
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("execution_id", executionId);
        command.Parameters.AddWithValue("kind", artifact.ArtifactKind);
        command.Parameters.AddWithValue("key", artifact.ObjectKey);
        command.Parameters.AddWithValue("sha256", artifact.Sha256);
        command.Parameters.AddWithValue("media_type", artifact.MediaType);
        command.Parameters.AddWithValue("size", artifact.SizeBytes);
        command.Parameters.AddWithValue("key_ref", keyReference);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertAuditAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long assetId,
        long executionId,
        string action,
        string reason,
        CancellationToken cancellationToken,
        string outcome = "Success")
    {
        const string sql = """
            INSERT INTO audit_log
              (actor_kind, category, action, asset_id, object_type, object_id, outcome,
               reason_masked, correlation_id)
            VALUES ('System', 'System', @action, @asset_id, 'zap_execution', @object_id,
                    @outcome, @reason, @correlation_id)
            """;
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("action", action);
        command.Parameters.AddWithValue("asset_id", assetId);
        command.Parameters.AddWithValue("object_id", executionId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("outcome", outcome);
        command.Parameters.AddWithValue("reason", reason);
        command.Parameters.AddWithValue("correlation_id", Guid.NewGuid());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SetSearchPathAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new("SET LOCAL search_path = htqlhdk, public", connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string NewCode(string prefix) =>
        $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..40];

    private sealed record DueSchedule(
        long Id,
        long AssetId,
        long ScopeId,
        DateTimeOffset DueAt,
        int PlannedDurationMinutes,
        long OperationsTeamId,
        string[] TargetUrls,
        string Frequency,
        string RecurrenceRule,
        string Timezone,
        DateTimeOffset? EffectiveUntil,
        long ProfileId,
        long ServiceUserId,
        string ProfileCode,
        int ProfileVersion);

    private sealed record JobRow(
        long ExecutionId,
        string ExecutionCode,
        long ScanId,
        long AssetId,
        long ServiceUserId,
        string[] TargetUrls,
        string SpiderKind,
        bool ActiveScanEnabled,
        string AuthenticationKind,
        string? AuthenticationHeaderName,
        string? AuthenticationSecretReference,
        int SpiderMaxMinutes,
        int PassiveWaitMaxMinutes,
        int ActiveScanMaxMinutes,
        int MaxRuleMinutes,
        int RequestDelayMilliseconds,
        int ThreadPerHost,
        DateTimeOffset StartedAt,
        int PlannedDurationMinutes);

    private sealed record ExpiredExecution(long ExecutionId, long ScanId, long AssetId);
}
