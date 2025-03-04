using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using Serilog;
using SqlSugar;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        IUserService userService;
        public Login()
        {
            userService = new UserService();
            InitializeComponent();

            if (!SQLite.CheckTableExists("Users"))
            {
                SQLite.CreateTable<User>();
                SQLite.InserTable<User>(new User() { Account = "admin", Password = "123456", Name = "管理员", Role = "admin" });
                SQLite.InserTable<User>(new User() { Account = "agent", Password = "123456", Name = "代理商", Role = "agent" });
                SQLite.InserTable<User>(new User() { Account = "agent1", Password = "123456", Name = "代理商1", Role = "agent" });

                SQLite.CreateTable<UserGroup>();
                SQLite.InserTable<UserGroup>(new UserGroup() { Name = "A组", AgentId = 2 });
                SQLite.InserTable<UserGroup>(new UserGroup() { Name = "B组", AgentId = 2 });
                SQLite.InserTable<UserGroup>(new UserGroup() { Name = "C组", AgentId = 3 });
                SQLite.InserTable<UserGroup>(new UserGroup() { Name = "D组", AgentId = 3 });

                SQLite.InserTable<User>(new User() { Account = "user1", Password = "123456", Name = "用户1", Role = "user", AgentId = 2 });
                SQLite.InserTable<User>(new User() { Account = "user2", Password = "123456", Name = "用户2", Role = "user", AgentId = 2 });

                SQLite.InserTable<User>(new User() { Account = "user3", Password = "123456", Name = "用户3", Role = "user", AgentId = 3 });
                SQLite.InserTable<User>(new User() { Account = "user4", Password = "123456", Name = "用户4", Role = "user", AgentId = 3 });
            }

            if (!SQLite.CheckTableExists("MediaGroups"))
            {
                SQLite.CreateTable<MediaGroup>();
            }

            if (!SQLite.CheckTableExists("Medias"))
            {
                SQLite.CreateTable<Media>();
                SQLite.InserTable<Media>(new Media() 
                { 
                    Name = "节目1", 
                    Status = 1, 
                    Type="常规节目",
                    Resolution= "1920*1080",
                    GroupId = 3,
                    UserId = 4 ,
                    Size ="3MB",
                    ScreensCount=1,
                    LastUpdatedTime = DateTime.Now,
                    CreatedSource="管理员",
                });
            }

            if (!SQLite.CheckTableExists("DeviceMedias"))
            {
                SQLite.CreateTable<DeviceMedia>();
                SQLite.InserTable<DeviceMedia>(new DeviceMedia()
                {
                    DeviceId = 1,
                    MediaId = 1,
                });
            }

            if (!SQLite.CheckTableExists("Devices"))
            {
                SQLite.CreateTable<Device>();
            }

            if (!SQLite.CheckTableExists("DeviceGroups"))
            {
                SQLite.CreateTable<DeviceGroup>();
            }

            if (!SQLite.CheckTableExists("DeviceControls"))
            {
                SQLite.CreateTable<DeviceControl>();
            }

            if (!SQLite.CheckTableExists("UserDetails"))
            {
                SQLite.CreateTable<UserDetail>();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string loginId = txtLoginId.Text;
            string password = passwordBox.Password;

            Log.Debug($"Login attempt with ID: {loginId}");

            if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Login ID and password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var loginUser = userService.GetUser(loginId, password).GetAwaiter().GetResult();
            if (loginUser != null)
            {
                Log.Information($"User {loginId} logged in successfully.");
                //MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                App.Current.MainWindow = new MainWindow(loginUser);
                App.Current.MainWindow.Show();
                this.Hide();
            }
            else
            {
                Log.Warning($"Failed login attempt for user {loginId}.");
                MessageBox.Show("Invalid login ID or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private User? ValidateCredentials(string loginId, string password)
        //{
        //    // Replace with actual validation logic, e.g., checking against a database 
        //    //if (!SQLite.CheckTableExists("Users"))
        //    //{
        //    //    SQLite.CreateTable<User>();
        //    //    SQLite.InserTable<User>(new User() { Account = "admin", Password = "123456", Name = "管理员", Role = "admin" });
        //    //}
        //    var result = userService.GetUser(string loginId, string password) SQLite.QueryTable<User>().Where(d => d.Account == loginId && password == "123456").First();
        //    return result;
        //}


        private void LanguageSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem selectedItem)
            {
                string selectedLanguage = selectedItem.Name.ToString()!;
                switch (selectedLanguage)
                {
                    case "zh_CN":
                        //ChangeLanguage("zh-CN");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Chinese;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_ZH").ToString();
                        Log.Debug("zh_CN");
                        break;
                    case "en_US":
                        //ChangeLanguage("en-US");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_EN").ToString();
                        Log.Debug("en_US");
                        break;
                    case "ja_JP":
                        //ChangeLanguage("ja-JP");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Japanese;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_JP").ToString();
                        Log.Debug("ja_JP");
                        break;
                    case "ko_KR":
                        //ChangeLanguage("ko-KR");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Korean;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_KO").ToString();
                        Log.Debug("ko_KR");
                        break;
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
