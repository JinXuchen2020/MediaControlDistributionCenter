using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// UserManage.xaml 的交互逻辑
    /// </summary>
    public partial class UserManage_old : UserControl
    {
        public ObservableCollection<User> Users { get; set; }
        public UserManage_old()
        {
            InitializeComponent();
            Users =
            [
                new User { Function = "功能1", Permission = "权限1", Group = "组1", ID = 1, CompanyName = "公司1", Region = "地区1" },
                new User { Function = "功能2", Permission = "权限2", Group = "组2", ID = 2, CompanyName = "公司2", Region = "地区2" },
                new User { Function = "功能3", Permission = "权限3", Group = "组3", ID = 3, CompanyName = "公司3", Region = "地区3" },
                new User { Function = "功能4", Permission = "权限4", Group = "组4", ID = 4, CompanyName = "公司4", Region = "地区4" },
                new User { Function = "功能5", Permission = "权限5", Group = "组5", ID = 5, CompanyName = "公司5", Region = "地区5" },
                new User { Function = "功能6", Permission = "权限6", Group = "组6", ID = 6, CompanyName = "公司6", Region = "地区6" },
                new User { Function = "功能7", Permission = "权限7", Group = "组7", ID = 7, CompanyName = "公司7", Region = "地区7" },
                new User { Function = "功能8", Permission = "权限8", Group = "组8", ID = 8, CompanyName = "公司8", Region = "地区8" },
                new User { Function = "功能9", Permission = "权限9", Group = "组9", ID = 9, CompanyName = "公司9", Region = "地区9" },
                new User { Function = "功能10", Permission = "权限10", Group = "组10", ID = 10, CompanyName = "公司10", Region = "地区10" }
            ];
            this.DataContext = this;
        }

        private void RoundRectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mainBorder.Child = new UserRegister();
        }

        private void UserEditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mainBorder.Child = new Setting();
        }
    }
    public class User
    {
        public string Function { get; set; }
        public string Permission { get; set; }
        public string Group { get; set; }
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string Region { get; set; }
    }

}
