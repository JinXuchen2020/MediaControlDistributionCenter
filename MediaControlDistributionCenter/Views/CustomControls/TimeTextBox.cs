using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MaterialDesignThemes.Wpf.Theme;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class TimeTextBox : Control
    {
        static TimeTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeTextBox), new FrameworkPropertyMetadata(typeof(TimeTextBox)));
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TimeTextBox), new PropertyMetadata("00:00:00",  (d, e) =>
            {
                if(d is TimeTextBox textBox && e.NewValue != null)
                {
                    var newValue = e.NewValue.ToString()!;
                    var regex = new Regex(@"(?<hour>[0-9]{1,2})[:](?<minute>[0-9]{1,2})[:](?<second>[0-9]{1,2})");
                    var match = regex.Match(newValue);

                    if (!match.Success)
                    {
                        if (int.TryParse(newValue, out int timeSeconds))
                        {
                            int seconds = timeSeconds % 60;
                            int minutes = (timeSeconds / 60) % 60;
                            int hours = timeSeconds / 60 / 60;
                            textBox.SetCurrentValue(TimelineProperty, timeSeconds);
                            textBox.SetCurrentValue(TextProperty, $"{hours:D2}:{minutes:D2}:{seconds:D2}");
                        }
                        else
                        {
                            textBox.SetCurrentValue(TimelineProperty, 0);
                            textBox.SetCurrentValue(TextProperty, "00:00:00");
                        }
                    }
                    else
                    {
                        int hours = int.Parse(match.Groups["hour"].Value);
                        int minutes = match.Groups["minute"].Success ? int.Parse(match.Groups["minute"].Value) : 0;
                        int seconds = match.Groups["second"].Success ? int.Parse(match.Groups["second"].Value) : 0;

                        // 确保小时、分钟和秒都在有效范围内
                        hours = Math.Min(hours, 100);
                        minutes = Math.Min(minutes, 59);
                        seconds = Math.Min(seconds, 59);

                        textBox.SetCurrentValue(TimelineProperty, hours * 3600 + minutes * 60 + seconds);
                        textBox.SetCurrentValue(TextProperty, $"{hours:D2}:{minutes:D2}:{seconds:D2}");
                    }
                }
            }));
        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register("Timeline", typeof(int), typeof(TimeTextBox), new PropertyMetadata(0));

        public static readonly DependencyProperty IsDisabledProperty =
            DependencyProperty.Register("IsDisabled", typeof(bool), typeof(TimeTextBox), new PropertyMetadata((d, e) =>
            {
                if (d is TimeTextBox textBox)
                {
                    if (e.OldValue == null)
                    {
                        textBox.IsDisabled = !textBox.IsEnabled;
                    }
                    else
                    {
                        textBox.IsEnabled = !textBox.IsDisabled;
                    }
                }
            }));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public int Timeline
        {
            get { return (int)GetValue(TimelineProperty); }
            set { SetValue(TimelineProperty, value); }
        }

        public bool IsDisabled
        {
            get { return (bool)GetValue(IsDisabledProperty); }
            set { SetValue(IsDisabledProperty, value); }
        }
    }
}
