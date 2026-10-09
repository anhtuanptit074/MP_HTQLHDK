using Htqlhdk.Worker.Automation;

namespace Htqlhdk.Worker.Tests;

[TestClass]
public sealed class EnvironmentZapSecretProviderTests
{
    [TestMethod]
    public async Task GetSecretAsyncAllowedPrefixReadsValue()
    {
        const string name = "HTQLHDK_ZAP_TEST_SECRET";
        Environment.SetEnvironmentVariable(name, "synthetic-test-value");
        try
        {
            string result = await new EnvironmentZapSecretProvider().GetSecretAsync(
                $"env:{name}", CancellationToken.None);
            Assert.AreEqual("synthetic-test-value", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [TestMethod]
    public async Task GetSecretAsyncArbitraryEnvironmentVariableIsRejected()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await new EnvironmentZapSecretProvider().GetSecretAsync("env:PATH", CancellationToken.None));
    }

    [TestMethod]
    public async Task GetSecretAsyncHeaderInjectionValueIsRejected()
    {
        const string name = "HTQLHDK_ZAP_TEST_INJECTION";
        Environment.SetEnvironmentVariable(name, "value\r\nInjected: true");
        try
        {
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
                await new EnvironmentZapSecretProvider().GetSecretAsync(
                    $"env:{name}", CancellationToken.None));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }
}
