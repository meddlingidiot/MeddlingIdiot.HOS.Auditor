namespace MeddlingIdiot.HOS.TimelineNavigator.Utilities
{
    public interface ILogger
    {
        public void Initialize(List<string> viewCategories);
        public void Initialize();
        //void Debug(string message);
        void Debug(string category, string message);
        Task SaveToFileAsync(string fileName);
        string GetResults();

    }
}
