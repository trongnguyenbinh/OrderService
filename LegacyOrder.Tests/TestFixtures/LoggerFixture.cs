namespace LegacyOrder.Tests.TestFixtures;

public static class LoggerFixture
{
    public static Mock<ILogger<T>> CreateLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    public static ILogger<T> CreateNullLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }
}

