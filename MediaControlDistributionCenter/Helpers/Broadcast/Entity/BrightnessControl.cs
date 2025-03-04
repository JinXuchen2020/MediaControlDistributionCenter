using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Broadcast.Entity
{
    public class BrightnessControl
    {
        public string Type { get; set; }  //控制类型
        public string DateTime { get; set; }  //执行时间
        public string JobPara { get; set; }  //重复方式
        public bool Enable { get; set; }  //命令执行  启动  or 取消
        public string DisableDateTime { get; set; }  //过期时间  有效日期
        public int Brightness { get; set; }  //命令参数 亮度
    }
}
