namespace MediaControlDistributionCenter.Models
{
    internal class AppRuntime
    {
        private static Lazy<AppRuntime>  _instance = new(() => new AppRuntime());
        public static AppRuntime Instance => _instance.Value;

        public string UserName { get; set; }


    }
}
