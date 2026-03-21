namespace cursor_dotnet_test.Persistence;

public class RedisCacheSettings
{
    public int DefaultTtlSeconds { get; set; } = 60;
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
}

public class CircuitBreakerSettings
{
    public double FailureRatio { get; set; } = 0.5;
    public int SamplingDurationSeconds { get; set; } = 10;
    public int MinimumThroughput { get; set; } = 3;
    public int BreakDurationSeconds { get; set; } = 30;
}
