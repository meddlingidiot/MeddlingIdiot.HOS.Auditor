

using System.Text;

namespace MeddlingIdiot.HOS.TimelineNavigator.Utilities
{
    public class NullLogger : ILogger
    {
        public void Initialize(List<string> viewCategories)
        {
        }

        public void Initialize()
        {
        }

        public void Debug(string category, string message)
        {
        }

        public Task SaveToFileAsync(string fileName)
        {
            return Task.CompletedTask;
        }

        public string GetResults()
        {
            return string.Empty;
        }
    }

    public class InMemoryLogger : ILogger
    {
        private readonly List<string> ViewCategories = new List<string>();
        private readonly StringBuilder _log = new StringBuilder();


        public InMemoryLogger() { }

        public void Initialize(List<string> viewCategories)
        {
            ViewCategories.Clear();
            foreach (var viewCategory in viewCategories)
            {
                ViewCategories.Add(viewCategory.Trim());
            }
        }

        public void Initialize()
        {
            _log.Clear();
            Initialize(new List<string> { });
        }


        public void Debug(string category, string message)
        {
            if (ViewCategories.Contains(category.Trim())
                || ViewCategories.Contains(LoggerCategories.All)
                || ViewCategories.Count == 0 /*Default is all*/)
            {
                _log.AppendLine($"{DateTime.UtcNow}|{category}|{message}");
                Console.WriteLine($"{DateTime.UtcNow}|{category}|{message}");
            }
        }

        public async Task SaveToFileAsync(string fileName)
        {
            await File.WriteAllTextAsync(fileName, _log.ToString());
        }

        public string GetResults()
        {
            return _log.ToString();
        }
        
        public override string ToString()
        {
            return _log.ToString();
        }
    }
}
