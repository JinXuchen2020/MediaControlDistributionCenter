using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaControlDistributionCenter.Views.UserManagement
{
    /// <summary>
    /// UserEditDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserEditDialog : UserControl
    {
        public UserEditDialog(UserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;
            var manageViewModel = App.ServicesProvider.GetRequiredService<UserManageViewModel>();
            if (manageViewModel.CurrentUser.Role == "agent")
            {
                userViewModel.AgentUserGroupId = userViewModel.SelectedGroupId;
            }
            else
            {
                userViewModel.AdminUserGroupId = userViewModel.SelectedGroupId;
            }

            manageViewModel.SaveUserCommand.Execute(userViewModel);
        }

        private void btnUpload_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ((sender as Border).DataContext as UserViewModel)!;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(filePath);
                this.Dispatcher.Invoke(async () =>
                {
                    var uploadService = Utility.GetService<IUploadService>();
                    if (uploadService is UploadServiceLocal local)
                    {
                        var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                        local.FtpClient = ftpClient;
                    }

                    await uploadService.UploadFile(filePath, $"{viewModel.Account}{extension}");

                    // 显示缩略图
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                    viewModel.LogoThumbnail = bitmap;
                    viewModel.Logo = filePath;
                    viewModel.LogoFileName = $"{viewModel.Account}{extension}";
                    viewModel.IsUpload = true;
                });
            }
        }
    }
}
