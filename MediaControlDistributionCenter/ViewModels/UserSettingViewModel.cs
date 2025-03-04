using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : ObservableObject
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        [ObservableProperty]
        public ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        public UserDetailViewModel userDetail;

        public UserSettingViewModel(UserViewModel userViewModel, UserViewModel loginUser)
        {
            CurrentUser = userViewModel;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            LoginUser = loginUser;

            var userDetailModel = SQLite.QueryTable<UserDetail>().Where(c => c.UserId == CurrentUser.Id).First();
            if(userDetailModel == null)
            {
                userDetail = new UserDetailViewModel()
                {
                    UserId = CurrentUser.Id,
                };
            }
            else
            {
                userDetail = new UserDetailViewModel(userDetailModel);
            }
        }
    }
}
