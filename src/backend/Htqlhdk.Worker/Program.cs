using Htqlhdk.Worker;
using Htqlhdk.Worker.Automation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Htqlhdk.Core;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
CoreConfiguration.LoadLocalEnvironment(builder.Environment.IsDevelopment());
builder.Configuration.AddEnvironmentVariables();
ZapConfiguration.ApplyEnvironmentOverrides(builder.Configuration);
builder.Services
    .AddOptions<ZapAutomationOptions>()
    .Bind(builder.Configuration.GetSection(ZapAutomationOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        value => value.HeartbeatSeconds < value.LeaseSeconds,
        "ZAP heartbeat interval must be shorter than the execution lease.")
    .ValidateOnStart();

ZapAutomationOptions startupOptions = new();
builder.Configuration.GetSection(ZapAutomationOptions.SectionName).Bind(startupOptions);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ZapPlanRenderer>();
builder.Services.AddSingleton<ZapReportInspector>();
builder.Services.AddSingleton<IZapSecretProvider, EnvironmentZapSecretProvider>();
builder.Services.AddSingleton<IZapArtifactStore, LocalZapArtifactStore>();
builder.Services.AddSingleton<IZapRunner, DockerZapRunner>();

bool core13 = Environment.GetEnvironmentVariable("HTQLHDK_SCHEMA_MODEL") == "core13";
if (core13)
{
    builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(CoreConfiguration.ConnectionString()));
    builder.Services.AddSingleton<CoreDatabase>();
    builder.Services.AddSingleton(_ => CoreConfiguration.Storage(builder.Environment.IsDevelopment()));
    builder.Services.AddSingleton<CoreService>();
    builder.Services.AddHostedService<Core13Worker>();
}
else if (startupOptions.Enabled)
{
    string connectionVariable = startupOptions.DatabaseConnectionStringEnvironmentVariable;
    string connectionString = ZapConfiguration.ResolveDatabaseConnectionString(connectionVariable);

    builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
    builder.Services.AddSingleton<ZapExecutionRepository>();
    builder.Services.AddSingleton<ZapAutomationOrchestrator>();
}

if (!core13) { builder.Services.AddHostedService<ZapAutomationWorker>(); }

using IHost host = builder.Build();
await host.RunAsync();
