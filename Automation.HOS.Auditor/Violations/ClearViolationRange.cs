namespace Automation.HOS.Violations
{
    public class ClearViolationRange
    {
        public ClearViolationRange(DateTime start, DateTime finish)
        {
            Start = start;
            Finish = finish;
        }

        public DateTime Start { get; private set; }
        public DateTime Finish { get; private set;}
    }
}
