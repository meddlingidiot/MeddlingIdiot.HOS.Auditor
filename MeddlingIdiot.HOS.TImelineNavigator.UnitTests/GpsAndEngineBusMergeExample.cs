using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests;

public class GpsAndEngineBusMergeExample
{
    [Test]
    public async Task StartOfDay_automatically_added_and_intermingles_with_GPS_and_EngineBus()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(daysToLoadBeforeEarliestTimestamp: 0));

        // Add GPS data around 6am - StartOfDay at midnight will be auto-generated
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 05:58:00"), 34.0522, -118.2437, null, null, null, null, "D12345", "T789")); // Los Angeles
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 06:02:00"), 34.0540, -118.2420, null, null, null, null, "D12345", "T789"));

        // Add EngineBus data - also contributes to StartOfDay generation
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 05:59:00"), 125430m, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 06:01:00"), 125432m, "D12345", "T789"));

        sut.Initialize();

        var moments = new List<Moment>();
        foreach (var moment in sut)
        {
            moments.Add(moment);
        }

        // Verify the moments are in chronological order
        await Assert.That(moments[0].Timestamp).IsEqualTo(DateTime.MinValue);
        await Assert.That(moments[1].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 00:00:00")); // Auto-generated StartOfDay at midnight
        await Assert.That(moments[2].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 05:58:00")); // GPS
        await Assert.That(moments[3].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 05:59:00")); // EngineBus
        await Assert.That(moments[4].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 06:01:00")); // EngineBus
        await Assert.That(moments[5].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 06:02:00")); // GPS

        // Verify types - StartOfDay is auto-generated at midnight
        await Assert.That(moments[1]).IsTypeOf<StartOfDayMoment>();
        await Assert.That(moments[2]).IsTypeOf<GpsMoment>();
        await Assert.That(moments[3]).IsTypeOf<EngineBusMoment>();
        await Assert.That(moments[4]).IsTypeOf<EngineBusMoment>();
        await Assert.That(moments[5]).IsTypeOf<GpsMoment>();

        // Verify StartOfDay is detected correctly
        sut.JumpTo(DateTime.Parse("01/15/2024 00:00:00"));
        await Assert.That(sut.IsStartOfDay).IsTrue();
    }

    [Test]
    public async Task GPS_and_EngineBus_data_intermingle_perfectly_during_truck_route()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(daysToLoadBeforeEarliestTimestamp: 0));

        // Simulate a truck route from Los Angeles to San Francisco
        // GPS coordinates along I-5 North (more frequent) with EngineBus odometer readings scattered throughout

        // Starting point - Los Angeles (morning)
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 06:00:00"), 34.0522, -118.2437, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 06:00:00"), 125000m, "D12345", "T789"));

        // Early drive - Burbank area
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 06:15:00"), 34.1031, -118.3300, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 06:30:00"), 34.1808, -118.3090, null, null, null, null,  "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 06:45:00"), 34.2500, -118.4400, null, null, null, null,  "D12345", "T789"));

        // Valencia area
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 07:00:00"), 125045m, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 07:05:00"), 34.3917, -118.5426, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 07:30:00"), 34.5553, -118.6453, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 08:00:00"), 34.8950, -118.9450, null, null, null, null, "D12345", "T789"));

        // Bakersfield approach
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 08:30:00"), 35.3733, -119.0187, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 08:30:00"), 125120m, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 09:00:00"), 35.6000, -119.2000, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 09:30:00"), 35.9500, -119.5000, null, null, null, null, "D12345", "T789"));

        // Fresno area
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 10:00:00"), 36.4500, -119.6500, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 10:15:00"), 125210m, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 10:20:00"), 36.7378, -119.7871, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 10:45:00"), 37.0000, -120.0000, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 11:15:00"), 37.3000, -120.4800, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 11:45:00"), 37.5000, -120.8500, null, null, null, null, "D12345", "T789"));

        // Modesto area - lunch break
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 12:00:00"), 37.6391, -120.9969, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 12:00:00"), 125305m, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 12:15:00"), 37.6391, -120.9969, null, null, null, null, "D12345", "T789")); // Still parked
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 12:30:00"), 125305m, "D12345", "T789")); // Still parked

        // Resume - Stockton area
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 12:45:00"), 37.7500, -121.1500, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 13:00:00"), 37.9577, -121.2908, null, null, null, null, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 13:20:00"), 38.1000, -121.5000, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 13:30:00"), 125340m, "D12345", "T789"));

        // Approaching Bay Area
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 13:45:00"), 38.0000, -122.0000, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 14:00:00"), 125365m, "D12345", "T789"));
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 14:10:00"), 37.7749, -122.4194, null, null, null, null, "D12345", "T789")); // San Francisco
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 14:20:00"), 37.7800, -122.4150, null, null, null, null, "D12345", "T789"));

        // Final destination
        sut.Add(new GpsMoment(DateTime.Parse("01/15/2024 14:30:00"), 37.7849, -122.4094, null, null, null, null, "D12345", "T789"));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/15/2024 14:30:00"), 125385m, "D12345", "T789"));

        sut.Initialize();

        var moments = new List<Moment>();
        foreach (var moment in sut)
        {
            moments.Add(moment);
        }

        // Verify chronological ordering
        for (int i = 1; i < moments.Count; i++)
        {
            Console.WriteLine($"{moments[i].Timestamp} - {moments[i].GetType().Name}");
            await Assert.That(moments[i].Timestamp).IsGreaterThanOrEqualTo(moments[i - 1].Timestamp);

            if (i == 1)
            {
                await Assert.That(moments[i].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 00:00:00"));
            }
            else if (i == 2)
            {
                await Assert.That(moments[i].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 06:00:00"));
            }
            else if (i == 3)
            {
                await Assert.That(moments[i].Timestamp).IsEqualTo(DateTime.Parse("01/15/2024 06:15:00"));
            }
        }

        // Count GPS and EngineBus moments - GPS is more frequent (realistic)
        var gpsMoments = moments.OfType<GpsMoment>().ToList();
        var engineBusMoments = moments.OfType<EngineBusMoment>().ToList();

        await Assert.That(gpsMoments.Count).IsEqualTo(24);
        await Assert.That(engineBusMoments.Count).IsEqualTo(5);
        await Assert.That(moments.Count).IsEqualTo(31);

        // Verify GPS coordinates are realistic
        sut.JumpTo(DateTime.Parse("01/15/2024 06:00:00"));
        await Assert.That(sut.Latitude).IsEqualTo(34.0522).Within(0.01); // Los Angeles
        await Assert.That(sut.Longitude).IsEqualTo(-118.2437).Within(0.01);
        await Assert.That(sut.Odometer).IsEqualTo(125000m);
        sut.JumpTo(DateTime.Parse("01/15/2024 14:30:00"));
        await Assert.That(sut.Latitude).IsEqualTo(37.7849).Within(0.01); // San Francisco
        await Assert.That(sut.Longitude).IsEqualTo(-122.4094).Within(0.01);
        await Assert.That(sut.Odometer).IsEqualTo(125385m);

        var momentTypes = moments.Skip(2).Take(20).Select(m => m.GetType().Name).ToList();
        var hasIntermingling = false;
        for (int i = 0; i < momentTypes.Count - 1; i++)
        {
            if (momentTypes[i] == "GpsMoment" && momentTypes[i + 1] == "EngineBusMoment" ||
                momentTypes[i] == "EngineBusMoment" && momentTypes[i + 1] == "GpsMoment")
            {
                hasIntermingling = true;
                break;
            }
        }
        await Assert.That(hasIntermingling).IsTrue();

        // Navigate and verify odometer is accessible
        sut.JumpTo(DateTime.Parse("01/15/2024 14:30:00"));
        await Assert.That(sut.Odometer).IsEqualTo(125385m);
    }

    [Test]
    public async Task GPS_and_EngineBus_with_simultaneous_timestamps_maintain_order()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(stopAtStartOfDay: false, daysToLoadBeforeEarliestTimestamp: 0));

        sut.Initialize();

        // Real-world scenario: GPS and EngineBus data captured at the exact same moment
        // Chicago to Milwaukee route

        sut.Add(new GpsMoment(DateTime.Parse("01/16/2024 09:00:00"), 41.8781, -87.6298, null, null, null, null, "D98765", "T456")); // Chicago
        sut.Add(new EngineBusMoment(DateTime.Parse("01/16/2024 09:00:00"), 87500m, "D98765", "T456"));

        sut.Add(new GpsMoment(DateTime.Parse("01/16/2024 09:30:00"), 42.0308, -87.6890, null, null, null, null, "D98765", "T456")); // Evanston
        sut.Add(new EngineBusMoment(DateTime.Parse("01/16/2024 09:30:00"), 87520m, "D98765", "T456"));

        sut.Add(new GpsMoment(DateTime.Parse("01/16/2024 10:00:00"), 42.3314, -87.8410, null, null, null, null, "D98765", "T456")); // Kenosha
        sut.Add(new EngineBusMoment(DateTime.Parse("01/16/2024 10:00:00"), 87555m, "D98765", "T456"));

        sut.Add(new GpsMoment(DateTime.Parse("01/16/2024 10:30:00"), 42.9634, -87.9065, null, null, null, null, "D98765", "T456")); // Racine
        sut.Add(new EngineBusMoment(DateTime.Parse("01/16/2024 10:30:00"), 87590m, "D98765", "T456"));

        sut.Add(new GpsMoment(DateTime.Parse("01/16/2024 11:00:00"), 43.0389, -87.9065, null, null, null, null, "D98765", "T456")); // Milwaukee
        sut.Add(new EngineBusMoment(DateTime.Parse("01/16/2024 11:00:00"), 87625m, "D98765", "T456"));

        var moments = new List<Moment>();
        foreach (var moment in sut)
        {
            moments.Add(moment);
        }

        // Verify all moments are present
        var gpsMoments = moments.OfType<GpsMoment>().ToList();
        var engineBusMoments = moments.OfType<EngineBusMoment>().ToList();

        await Assert.That(gpsMoments.Count).IsEqualTo(5);
        await Assert.That(engineBusMoments.Count).IsEqualTo(0);
        await Assert.That(moments.Count).IsEqualTo(6);

        // Verify chronological order maintained
        for (int i = 1; i < moments.Count; i++)
        {
            await Assert.That(moments[i].Timestamp).IsGreaterThanOrEqualTo(moments[i - 1].Timestamp);
        }

        // Navigate through timeline and verify both GPS and odometer data are accessible
        sut.JumpTo(DateTime.Parse("01/16/2024 09:00:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/16/2024 09:00:00"));
        await Assert.That(sut.Odometer).IsEqualTo(87500m);

        sut.JumpTo(DateTime.Parse("01/16/2024 11:00:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/16/2024 11:00:00"));
        await Assert.That(sut.Odometer).IsEqualTo(87625m);
    }
}
