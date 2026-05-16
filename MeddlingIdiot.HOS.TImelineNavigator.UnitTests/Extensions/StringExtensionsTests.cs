using MeddlingIdiot.HOS.TimelineNavigator.Extensions;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Extensions;

public class StringExtensionsTests
{
    // ── ToDateTimeOffsetOrNull ────────────────────────────────────────────────

    [Test]
    public async Task ToDateTimeOffsetOrNull_ValidString_ReturnsParsedValue()
    {
        var result = "2023-01-27T08:00:00+00:00".ToDateTimeOffsetOrNull();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value.Year).IsEqualTo(2023);
        }
    }

    [Test]
    public async Task ToDateTimeOffsetOrNull_InvalidString_ReturnsNull()
    {
        var result = "not-a-date".ToDateTimeOffsetOrNull();
        await Assert.That(result).IsNull();
    }

    // ── ToDateTimeOrNull ──────────────────────────────────────────────────────

    [Test]
    public async Task ToDateTimeOrNull_ValidString_ReturnsParsedValue()
    {
        var result = "01/27/2023 08:00:00".ToDateTimeOrNull();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        }
    }

    [Test]
    public async Task ToDateTimeOrNull_InvalidString_ReturnsNull()
    {
        var result = "not-a-date".ToDateTimeOrNull();
        await Assert.That(result).IsNull();
    }

    // ── ToDoubleOrNull ────────────────────────────────────────────────────────

    [Test]
    public async Task ToDoubleOrNull_ValidString_ReturnsParsedValue()
    {
        var result = "3.14".ToDoubleOrNull();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(3.14).Within(0.0001);
        }
    }

    [Test]
    public async Task ToDoubleOrNull_InvalidString_ReturnsNull()
    {
        var result = "not-a-number".ToDoubleOrNull();
        await Assert.That(result).IsNull();
    }

    // ── ToIntOrNull ───────────────────────────────────────────────────────────

    [Test]
    public async Task ToIntOrNull_ValidString_ReturnsParsedValue()
    {
        var result = "42".ToIntOrNull();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(42);
        }
    }

    [Test]
    public async Task ToIntOrNull_InvalidString_ReturnsNull()
    {
        var result = "not-an-int".ToIntOrNull();
        await Assert.That(result).IsNull();
    }

    // ── ToIntOrDefault ────────────────────────────────────────────────────────

    [Test]
    public async Task ToIntOrDefault_ValidString_ReturnsParsedValue()
    {
        var result = "7".ToIntOrDefault(defaultValue: 99);
        await Assert.That(result).IsEqualTo(7);
    }

    [Test]
    public async Task ToIntOrDefault_InvalidString_ReturnsDefault()
    {
        var result = "not-an-int".ToIntOrDefault(defaultValue: 99);
        await Assert.That(result).IsEqualTo(99);
    }

    // ── StripLineEndChars ─────────────────────────────────────────────────────

    [Test]
    public async Task StripLineEndChars_Null_ReturnsNull()
    {
        var result = ((string?)null).StripLineEndChars();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task StripLineEndChars_StringWithNewlines_RemovesNewlines()
    {
        var result = "line1\nline2\r\nline3".StripLineEndChars();
        await Assert.That(result).IsEqualTo("line1line2line3");
    }

    [Test]
    public async Task StripLineEndChars_StringWithTabs_RemovesTabs()
    {
        var result = "col1\tcol2".StripLineEndChars();
        await Assert.That(result).IsEqualTo("col1col2");
    }

    [Test]
    public async Task StripLineEndChars_StringWithNoSpecialChars_ReturnsUnchanged()
    {
        var result = "hello world".StripLineEndChars();
        await Assert.That(result).IsEqualTo("hello world");
    }
}
