namespace MeddlingIdiot.HOS.RestTimelineBuilders
{
    internal interface IRestTimelinePairer
    {
        void PairSleeperSplits(CancellationToken cancellationToken = default);
    }
}
