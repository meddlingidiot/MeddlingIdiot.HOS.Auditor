using MeddlingIdiot.HOS.Queries;

namespace MeddlingIdiot.HOS.Auditor.UnitTests.Queries;

public class DaySummaryTests
{
    private static readonly DateTime Date = DateTime.Parse("2024-03-10");
    private static readonly TimeSpan WindowLimit = TimeSpan.FromHours(70);
    private const int DaysInWindow = 7;
    // oldest day in window = Date.AddDays(-DaysInWindow - 1) = 2024-03-02
    private static readonly DateTime OldestDay = Date.AddDays(-DaysInWindow - 1);

    [Test]
    public async Task Properties_AreAssignedFromConstructor()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>();
        var sut = new DaySummary(Date, TimeSpan.FromHours(8), TimeSpan.FromHours(50),
            WindowLimit, DaysInWindow, dailyHours);

        await Assert.That(sut.Date).IsEqualTo(Date);
        await Assert.That(sut.HoursForDay).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(sut.HoursInWindow).IsEqualTo(TimeSpan.FromHours(50));
        await Assert.That(sut.DailyHours).IsEqualTo(dailyHours);
    }

    [Test]
    public async Task HoursAvailableToday_WhenUnderLimit_ReturnsRemainingHours()
    {
        var sut = new DaySummary(Date, TimeSpan.FromHours(8), TimeSpan.FromHours(50),
            WindowLimit, DaysInWindow, new Dictionary<DateTime, TimeSpan>());

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.FromHours(20));
    }

    [Test]
    public async Task HoursAvailableToday_WhenAtLimit_ReturnsZero()
    {
        var sut = new DaySummary(Date, TimeSpan.FromHours(10), TimeSpan.FromHours(70),
            WindowLimit, DaysInWindow, new Dictionary<DateTime, TimeSpan>());

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task HoursAvailableToday_WhenOverLimit_ReturnsZero()
    {
        var sut = new DaySummary(Date, TimeSpan.FromHours(10), TimeSpan.FromHours(75),
            WindowLimit, DaysInWindow, new Dictionary<DateTime, TimeSpan>());

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task HoursAvailableTomorrow_WhenOldestDayHasHours_AddsThatToAvailability()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>
        {
            { OldestDay, TimeSpan.FromHours(10) }
        };
        // Window = 60h used, oldest day = 10h falling off → tomorrow window = 50h
        var sut = new DaySummary(Date, TimeSpan.FromHours(8), TimeSpan.FromHours(60),
            WindowLimit, DaysInWindow, dailyHours);

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.FromHours(20));
    }

    [Test]
    public async Task HoursAvailableTomorrow_WhenOldestDayHasNoHours_MatchesToday()
    {
        var sut = new DaySummary(Date, TimeSpan.FromHours(8), TimeSpan.FromHours(60),
            WindowLimit, DaysInWindow, new Dictionary<DateTime, TimeSpan>());

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.FromHours(10));
    }

    [Test]
    public async Task HoursAvailableTomorrow_WhenAtLimitTodayAndOldestDayHasHours_ReturnsOldestDayHours()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>
        {
            { OldestDay, TimeSpan.FromHours(10) }
        };
        var sut = new DaySummary(Date, TimeSpan.FromHours(10), TimeSpan.FromHours(70),
            WindowLimit, DaysInWindow, dailyHours);

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.Zero);
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.FromHours(10));
    }

    [Test]
    public async Task HoursAvailableTomorrow_WhenOverLimitAndOldestDayPartiallyRecovers_ReturnsPartialHours()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>
        {
            { OldestDay, TimeSpan.FromHours(10) }
        };
        // 75h used, limit 70h → today available = 0; tomorrow = 75-10=65h used → available = 5h
        var sut = new DaySummary(Date, TimeSpan.FromHours(10), TimeSpan.FromHours(75),
            WindowLimit, DaysInWindow, dailyHours);

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.Zero);
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.FromHours(5));
    }

    [Test]
    public async Task HoursAvailableTomorrow_WhenOverLimitAndOldestDayCannotFullyRecover_ReturnsZero()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>
        {
            { OldestDay, TimeSpan.FromHours(5) }
        };
        // 80h used, limit 70h → today available = 0; tomorrow = 80-5=75h used → still over limit
        var sut = new DaySummary(Date, TimeSpan.FromHours(10), TimeSpan.FromHours(80),
            WindowLimit, DaysInWindow, dailyHours);

        await Assert.That(sut.HoursAvailableToday).IsEqualTo(TimeSpan.Zero);
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task HoursAvailableTomorrow_IgnoresNonOldestDaysInDictionary()
    {
        var dailyHours = new Dictionary<DateTime, TimeSpan>
        {
            { Date.AddDays(-1), TimeSpan.FromHours(10) }, // yesterday — not oldest, should be ignored
            { OldestDay, TimeSpan.FromHours(6) }
        };
        var sut = new DaySummary(Date, TimeSpan.FromHours(8), TimeSpan.FromHours(60),
            WindowLimit, DaysInWindow, dailyHours);

        // tomorrow window = 60 - 6 = 54h → available = 16h
        await Assert.That(sut.HoursAvailableTomorrow).IsEqualTo(TimeSpan.FromHours(16));
    }
}
