using ServiceLayer.Services;
using System.Text.RegularExpressions;

public class LibraryLoggerTests
{
    private static readonly Regex InfoRegex =
        new(@"^\[INFO\]\[\d{2}:\d{2}:\d{2}\] .+", RegexOptions.Compiled);

    private static readonly Regex WarnRegex =
        new(@"^\[WARN\]\[\d{2}:\d{2}:\d{2}\] .+", RegexOptions.Compiled);

    private static readonly Regex ErrorRegex =
        new(@"^\[ERROR\]\[\d{2}:\d{2}:\d{2}\] .+", RegexOptions.Compiled);

    [Fact]
    public void LogInformation_WritesExpectedPrefixAndResetsColor()
    {
        // Arrange
        var logger = new LibraryLogger();
        var originalColor = Console.ForegroundColor;

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            // Act
            logger.LogInformation("hello info");

            // Assert output
            var output = sw.ToString().TrimEnd();
            Assert.Matches(InfoRegex, output);
            Assert.Contains("hello info", output);

            // Assert color reset
            Assert.Equal(originalColor, Console.ForegroundColor);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }
    }

    [Fact]
    public void LogWarning_WritesExpectedPrefixAndResetsColor()
    {
        // Arrange
        var logger = new LibraryLogger();
        var originalColor = Console.ForegroundColor;

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            // Act
            logger.LogWarning("careful!");

            // Assert output
            var output = sw.ToString().TrimEnd();
            Assert.Matches(WarnRegex, output);
            Assert.Contains("careful!", output);

            // Assert color reset
            Assert.Equal(originalColor, Console.ForegroundColor);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }
    }

    [Fact]
    public void LogError_WithoutException_WritesOneLineAndResetsColor()
    {
        // Arrange
        var logger = new LibraryLogger();
        var originalColor = Console.ForegroundColor;

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            // Act
            logger.LogError("boom");

            // Assert: should contain only the ERROR line (no stacktrace line)
            var output = sw.ToString()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.Single(output);
            Assert.Matches(ErrorRegex, output[0]);
            Assert.Contains("boom", output[0]);

            // Assert color reset
            Assert.Equal(originalColor, Console.ForegroundColor);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }
    }

    [Fact]
    public void LogError_WithException_WritesErrorLineAndStackTrace()
    {
        // Arrange
        var logger = new LibraryLogger();
        var originalColor = Console.ForegroundColor;

        Exception exWithStackTrace;
        try
        {
            ThrowForStackTrace();
            throw new Exception("Unreachable");
        }
        catch (Exception ex)
        {
            exWithStackTrace = ex;
        }

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            // Act
            logger.LogError("boom", exWithStackTrace);

            // Assert: 2+ lines (error + stack trace)
            var output = sw.ToString();
            var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.True(lines.Length >= 2, "Expected at least 2 lines: error line + stacktrace.");
            Assert.Matches(ErrorRegex, lines[0]);
            Assert.Contains("boom", lines[0]);

            // stack trace existence (nu ne bazăm pe text exact)
            Assert.Contains(nameof(ThrowForStackTrace), output);

            // Assert color reset
            Assert.Equal(originalColor, Console.ForegroundColor);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LogInformation_AllowsNullOrEmptyMessage(string message)
    {
        var logger = new LibraryLogger();

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            var ex = Record.Exception(() => logger.LogInformation(message));
            Assert.Null(ex);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LogError_AllowsNullOrEmptyMessage(string message)
    {
        var logger = new LibraryLogger();

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            var ex = Record.Exception(() => logger.LogError(message));
            Assert.Null(ex);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static void ThrowForStackTrace()
        => throw new InvalidOperationException("test exception");
}
