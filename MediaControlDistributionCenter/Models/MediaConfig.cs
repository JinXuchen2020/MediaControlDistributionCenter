using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public class MediaConfig
    {
        public double Left { get; set; }

        public double Top { get; set; }

        public double Ratio { get; set; }

        public ProgramDto Program { get; set; }

        public List<MediaPage> Pages { get; set; }
    }

    public class MediaPage
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public int Order { get; set; }

        public string ThumbnailFilePath { get; set; }

        public bool IsHasValidity { get; set; }

        public DateTime? ValidStartDate { get; set; }

        public DateTime? ValidEndDate { get; set; }

        public int PlayCount { get; set; }

        public int PlayGap { get; set; }

        public string AdPlayMode { get; set; }

        public List<Scheduler> Schedulers{ get; set; }

        public List<BaseComponent> Components{ get; set; }
    }

    public class Scheduler
    {        
        public int Id { get; set; }
        
        public string StartTime { get; set; } = "00:00:00";

        
        public string EndTime { get; set; } = "23:59:59";

        
        public List<int> ScheduleDays { get; set; }
    }


    public class BaseComponent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ZIndex { get; set; }

        public double Left { get; set; }

        
        public double Top { get; set; }

        
        public double Width { get; set; }

        
        public double Height { get; set; }

        public string Source { get; set; }

        public double Timeline { get; set; }

        public int PlayCount { get; set; }

        public string PlayDuration { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual MediaType Type { get; set; }
    }

    public class VideoComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Video;

        public string PlayMode { get; set; }

        public bool IsClip { get; set; }
    }

    public class ImageComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Image;

        public int EffectDuration { get; set; }  //特效时长    -毫秒
        
        public string ComponentEffect { get; set; } //"上下展开",
    }

    public partial class TextComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Text;
        
        public string Background { get; set; }

        public string TextColor { get; set; }
        
        public string PlayMode{ get; set; } //"翻页"

        public string Direction{ get; set; } //"向右滚动"

        public int EffectDuration { get; set; } //特效时长    -毫秒   文本翻页时才用到

        public string ComponentEffect { get; set; } //入场特效               文本翻页时才用到     

        public int RollingSpeed { get; set; }  //滚动速度档位        一共1-10个档位

        public double TextSize { get; set; } //20,                            //字体大小
        
        public bool IsLoopEnabled { get; set; } // true,                //是否首尾相接
        
        public double LetterSpacing{ get; set; } //10",                  //字体间距
        
        public double LineSpacing { get; set; } //16", 

        public string RtfFilePath { get; set; }


    }

    public class ColorTextComponent : BaseComponent
    {
        public override MediaType Type => MediaType.ColorText;

        public string Background { get; set; }

        public string FontFamily { get; set; }

        public int FontSize { get; set; } = 12;

        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }

        public bool IsUnderline { get; set; }

        public double LetterSpacing { get; set; }

        public string ComponentEffect { get; set; }

        public string Direction { get; set; }

        public int RollingSpeed { get; set; }

        public bool IsLoopEnabled { get; set; }
    }

    public class WebComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Web;
    }

    public class WordComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Word;
        
        //public int EffectDuration { get; set; }
        
        //public string ComponentEffect { get; set; }
        
        public int PageDuration { get; set; }
    }

    public class RssComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Rss;

        public int Interval { get; set; }

        public IList<RssContent> Contents { get; set; }

        public string PlayMode { get; set; }

        public string Direction { get; set; } //"向右滚动"

        public int RollingSpeed { get; set; }  //滚动速度档位        一共1-10个档位

        public int EffectDuration { get; set; } //特效时长    -毫秒   文本翻页时才用到

        public string ComponentEffect { get; set; } //入场特效               文本翻页时才用到
    }

    public class RssContent
    {
        public string FieldName { get; set; }

        public string FontFamily { get; set; }

        public int FontSize { get; set; } = 12;

        public string FontColor { get; set; } = "White";

        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }

        public bool IsUnderline { get; set; }
    }


    public class StreamComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Stream;
    }

    public class HdmiComponent : BaseComponent
    {
        public override MediaType Type => MediaType.Hdmi;
    }
}
