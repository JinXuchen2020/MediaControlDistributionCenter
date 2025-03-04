using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaConfigViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private double width;

        [ObservableProperty]
        private double height;

        [ObservableProperty]
        private double left;

        [ObservableProperty]
        private double top;

        [ObservableProperty]
        private double ratio;

        [ObservableProperty]
        private ObservableCollection<MediaPageViewModel> pages;

        public MediaConfigViewModel(MediaConfig config)
        {
            id = config.Id;
            name = config.Name;
            width = config.Width;
            height = config.Height;
            left = config.Left;
            top = config.Top;
            ratio = config.Ratio;
            pages = new ObservableCollection<MediaPageViewModel>(config.Pages.OrderBy(c => c.Order).Select(c => new MediaPageViewModel(c, config.Ratio)));
        }

        public MediaConfig ToModel()
        {
            return new MediaConfig
            {
                Id = Id,
                Name = Name,
                Width = Width,
                Height = Height,
                Left = Left,
                Top = Top,
                Ratio = Ratio,
                Pages = Pages.Select(c => c.ToModel(Ratio)).ToList()
            };
        }
    }

    public partial class MediaPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int order;

        [ObservableProperty]
        private string thumbnail;

        [ObservableProperty]
        private bool isHasValidity;

        [ObservableProperty]
        private DateTime? validStartDate;

        [ObservableProperty]
        private DateTime? validEndDate;

        [ObservableProperty]
        private int playCount;

        [ObservableProperty]
        private string effect;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private ObservableCollection<SchedulerViewModel> schedulers;

        [ObservableProperty]
        private ObservableCollection<BaseComponentViewModel?> components;

        public MediaPageViewModel(MediaPage mediaPage, double ratio = 1)
        {
            id = mediaPage.Id;
            name = mediaPage.Name;
            order = mediaPage.Order;
            isHasValidity = mediaPage.IsHasValidity;
            validStartDate = mediaPage.ValidStartDate;
            validEndDate = mediaPage.ValidEndDate;
            playCount = mediaPage.PlayCount;
            schedulers = new ObservableCollection<SchedulerViewModel>(mediaPage.Schedulers.Select(c => new SchedulerViewModel(c.Id, c.StartTime, c.EndTime, c.ScheduleDays)));
            components = new ObservableCollection<BaseComponentViewModel?>(mediaPage.Components.Select(c =>
            {
                BaseComponentViewModel? result = null;

                switch (c.Type)
                {
                    case MediaType.Image:
                        result = new ImageComponentViewModel((ImageComponent)c, ratio);
                        break;
                    case MediaType.Video:
                        result = new VideoComponentViewModel((VideoComponent)c, ratio);
                        break;
                    case MediaType.Text:
                        result = new TextComponentViewModel((TextComponent)c, ratio);
                        break;
                }

                return result;
            }));
        }

        public MediaPage ToModel(double ratio)
        {
            return new MediaPage
            {
                Id = Id,
                Name = Name,
                Order = Order,
                IsHasValidity = IsHasValidity,
                ValidStartDate = ValidStartDate,
                ValidEndDate = ValidEndDate,
                PlayCount = PlayCount,
                Schedulers = Schedulers.Select(c => c.ToModel()).ToList(),
                Components = Components.Select(c => c!.ToModel(ratio)).ToList()
            };
        }
    }

    public partial class SchedulerViewModel: ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private DateTime startTime;

        [ObservableProperty]
        private DateTime endTime;

        [ObservableProperty]
        private ObservableCollection<int> scheduleDays;

        public List<SchedulerDayViewModel> AllDays { get; set; }

        public SchedulerViewModel(int id, string startTime, string endTime, IEnumerable<int> scheduleDays)
        {
            this.id = id;
            this.startTime = string.IsNullOrEmpty(startTime)? DateTime.MinValue : DateTime.Parse(startTime);
            this.endTime = string.IsNullOrEmpty(endTime) ? DateTime.MinValue : DateTime.Parse(endTime);
            this.scheduleDays = new ObservableCollection<int>(scheduleDays);
            AllDays = new List<SchedulerDayViewModel>
            {
                new SchedulerDayViewModel
                {
                    Id = 1,
                    SchedulerId = id,
                    DisplayName = "一",
                    IsSelected = this.scheduleDays.Contains(1),
                },
                new SchedulerDayViewModel
                {
                    Id = 2,
                    SchedulerId = id,
                    DisplayName = "二",
                    IsSelected = this.scheduleDays.Contains(2),
                },
                new SchedulerDayViewModel
                {
                    Id = 3,
                    SchedulerId = id,
                    DisplayName = "三",
                    IsSelected = this.scheduleDays.Contains(3),
                },
                new SchedulerDayViewModel
                {
                    Id = 4,
                    SchedulerId = id,
                    DisplayName = "四",
                    IsSelected = this.scheduleDays.Contains(4),
                },
                new SchedulerDayViewModel
                {
                    Id = 5,
                    SchedulerId = id,
                    DisplayName = "五",
                    IsSelected = this.scheduleDays.Contains(5),
                },
                new SchedulerDayViewModel
                {
                    Id = 6,
                    SchedulerId = id,
                    DisplayName = "六",
                    IsSelected = this.scheduleDays.Contains(6),
                },
                new SchedulerDayViewModel
                {
                    Id = 7,
                    SchedulerId = id,
                    DisplayName = "日",
                    IsSelected = this.scheduleDays.Contains(7),
                }
            };
        }

        public Scheduler ToModel()
        {
            return new Scheduler
            {
                Id = Id,
                StartTime = StartTime.ToString("HH:mm:ss"),
                EndTime = EndTime.ToString("HH:mm:ss"),
                ScheduleDays = ScheduleDays.ToList()
            };
        }
    }

    public partial class SchedulerDayViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string displayName;

        [ObservableProperty]
        private bool isSelected;

        public int SchedulerId { get; set; }

        public int ToModel()
        {
            return Id;
        }
    }
}
