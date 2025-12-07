namespace LegacyOrder.Tests.Helpers;

public static class MockHelpers
{
    public static void VerifyLogging<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel logLevel,
        Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }

    public static void VerifyLoggingContains<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel logLevel,
        string expectedMessage)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce());
    }
}

