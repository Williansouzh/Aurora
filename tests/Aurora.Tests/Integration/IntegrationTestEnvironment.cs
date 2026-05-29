namespace Aurora.Tests.Integration;

public static class IntegrationTestEnvironment
{
    public static bool ShouldRunDockerTests() =>
        string.Equals(
            Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);
}
