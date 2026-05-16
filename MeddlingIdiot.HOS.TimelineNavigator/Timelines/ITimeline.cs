using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.TimelineNavigator.Timelines
{

    internal interface ITimeline
    {
        void Initialize();
        void Clear();
        void Next();
        void Prior();
        bool IsBeginningOfTime();
        bool IsEndOfTime();

        Moment BaseMoment { get; }

        void MoveOnOrBefore(DateTime searchTimestamp);
        void MoveOnOrAfter(DateTime searchTimestamp);

    }

    public interface ITimeline<T> where T : Moment
    {
        void Add(T value);
        void Upsert(T value);
        void Initialize();
        void Clear();
        void Next();
        void Prior();
        bool IsBeginningOfTime();
        bool IsEndOfTime();

        T CurrentMoment { get; }

        void MoveOnOrBefore(DateTime searchTimestamp);
        void MoveOnOrAfter(DateTime searchTimestamp);
    }
}
