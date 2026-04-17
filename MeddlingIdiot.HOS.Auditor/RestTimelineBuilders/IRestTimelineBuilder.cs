namespace MeddlingIdiot.HOS.RestTimelineBuilders
{
    internal interface IRestTimelineBuilder
    {
        void BuildTimeline(CancellationToken cancellationToken = default);
    }
}
