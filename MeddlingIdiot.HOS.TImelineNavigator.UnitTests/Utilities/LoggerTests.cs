using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Utilities;

public class LoggerTests
{
    // ── NullLogger ────────────────────────────────────────────────────────────

    [Test]
    public void NullLogger_InitializeWithCategories_DoesNotThrow()
    {
        var logger = new NullLogger();
        logger.Initialize(new List<string> { LoggerCategories.All });
    }

    [Test]
    public void NullLogger_Initialize_DoesNotThrow()
    {
        var logger = new NullLogger();
        logger.Initialize();
    }

    [Test]
    public void NullLogger_Debug_DoesNotThrow()
    {
        var logger = new NullLogger();
        logger.Debug(LoggerCategories.General, "msg");
    }

    [Test]
    public async Task NullLogger_SaveToFileAsync_ReturnsCompletedTask()
    {
        var logger = new NullLogger();
        await logger.SaveToFileAsync("ignored.txt");
    }

    [Test]
    public async Task NullLogger_GetResults_ReturnsEmptyString()
    {
        var logger = new NullLogger();
        await Assert.That(logger.GetResults()).IsEqualTo(string.Empty);
    }

    // ── InMemoryLogger ────────────────────────────────────────────────────────

    [Test]
    public async Task InMemoryLogger_GetResults_IsEmptyBeforeAnyDebugCalls()
    {
        var logger = new InMemoryLogger();
        await Assert.That(logger.GetResults()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task InMemoryLogger_Debug_LogsMessage_WhenNoCategoriesSet()
    {
        var logger = new InMemoryLogger();
        logger.Debug(LoggerCategories.General, "hello");
        await Assert.That(logger.GetResults()).Contains("hello");
    }

    [Test]
    public async Task InMemoryLogger_Debug_LogsMessage_WhenCategoryMatches()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { LoggerCategories.General });
        logger.Debug(LoggerCategories.General, "matched");
        await Assert.That(logger.GetResults()).Contains("matched");
    }

    [Test]
    public async Task InMemoryLogger_Debug_LogsMessage_WhenAllCategorySet()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { LoggerCategories.All });
        logger.Debug(LoggerCategories.Rule, "all-category-msg");
        await Assert.That(logger.GetResults()).Contains("all-category-msg");
    }

    [Test]
    public async Task InMemoryLogger_Debug_DoesNotLog_WhenCategoryDoesNotMatch()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { LoggerCategories.General });
        logger.Debug(LoggerCategories.Rule, "filtered-out");
        await Assert.That(logger.GetResults()).DoesNotContain("filtered-out");
    }

    [Test]
    public async Task InMemoryLogger_Debug_TrimsCategory_BeforeMatching()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { $"  {LoggerCategories.General}  " });
        logger.Debug(LoggerCategories.General, "trimmed-match");
        await Assert.That(logger.GetResults()).Contains("trimmed-match");
    }

    [Test]
    public async Task InMemoryLogger_Debug_IncludesCategoryAndMessageInLog()
    {
        var logger = new InMemoryLogger();
        logger.Debug(LoggerCategories.Violations, "violation-detail");
        var result = logger.GetResults();
        await Assert.That(result).Contains(LoggerCategories.Violations);
        await Assert.That(result).Contains("violation-detail");
    }

    [Test]
    public async Task InMemoryLogger_Initialize_ClearsLog()
    {
        var logger = new InMemoryLogger();
        logger.Debug(LoggerCategories.General, "before-clear");
        logger.Initialize();
        await Assert.That(logger.GetResults()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task InMemoryLogger_Initialize_ClearsCategories_SoDefaultIsAllAgain()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { LoggerCategories.General });
        logger.Initialize(); // clears categories → default is all
        logger.Debug(LoggerCategories.Rule, "after-reset");
        await Assert.That(logger.GetResults()).Contains("after-reset");
    }

    [Test]
    public async Task InMemoryLogger_InitializeWithCategories_ReplacesExistingCategories()
    {
        var logger = new InMemoryLogger();
        logger.Initialize(new List<string> { LoggerCategories.General });
        logger.Initialize(new List<string> { LoggerCategories.Rule });
        logger.Debug(LoggerCategories.General, "old-category");
        logger.Debug(LoggerCategories.Rule, "new-category");
        var result = logger.GetResults();
        await Assert.That(result).DoesNotContain("old-category");
        await Assert.That(result).Contains("new-category");
    }

    [Test]
    public async Task InMemoryLogger_ToString_MatchesGetResults()
    {
        var logger = new InMemoryLogger();
        logger.Debug(LoggerCategories.General, "tostring-test");
        await Assert.That(logger.ToString()).IsEqualTo(logger.GetResults());
    }

    [Test]
    public async Task InMemoryLogger_SaveToFileAsync_WritesContentToFile()
    {
        var logger = new InMemoryLogger();
        logger.Debug(LoggerCategories.General, "file-content");
        var path = Path.GetTempFileName();
        try
        {
            await logger.SaveToFileAsync(path);
            var written = await File.ReadAllTextAsync(path);
            await Assert.That(written).Contains("file-content");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
