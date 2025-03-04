using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        public int id;

        [ObservableProperty]
        public int userId;

        [ObservableProperty]
        public string? timeZone;

        [ObservableProperty]
        public string? logo;

        [ObservableProperty]
        public BitmapImage? logoThumbnail;

        [ObservableProperty]
        public string? companyName;

        [ObservableProperty]
        public string? tagLine;

        [ObservableProperty]
        public bool isUpload;

        public UserDetailViewModel(UserDetail? model = null)
        {
            id = model?.Id ?? 0;
            userId = model?.UserId ?? 0;
            timeZone = model?.TimeZone;
            logo = model?.Logo;
            companyName = model?.CompanyName;
            tagLine = model?.TagLine;
            isUpload = model?.Logo != null;
            logoThumbnail = GetThumbnail();
        }

        [RelayCommand]
        private void Reset()
        {
            var model = SQLite.QueryTable<UserDetail>().Where(c => c.UserId == UserId).First();
            if (model == null)
            {
                Id = 0;
                UserId = UserId;
                TimeZone = null;
                Logo = null;
                CompanyName = null;
                TagLine = null;
                IsUpload = false;
            }
            else
            {
                Id = model.Id;
                UserId = model.UserId;
                TimeZone = model.TimeZone;
                Logo = model.Logo;
                CompanyName = model.CompanyName;
                TagLine = model.TagLine;
                IsUpload = model.Logo != null;
                LogoThumbnail = GetThumbnail();
            }
        }

        public UserDetail ToModel()
        {
            return new UserDetail
            {
                Id = Id,
                UserId = UserId,
                TimeZone = TimeZone,
                Logo = Logo,
                CompanyName= CompanyName,
                TagLine = TagLine
            };
        }

        public BitmapImage? GetThumbnail()
        {
            if(!string.IsNullOrEmpty(Logo) && File.Exists(Logo))
            {
                return new BitmapImage(new Uri(Logo));
            }

            return null;
        }
    }
}
