using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Broadcast
{
    public static class CommunicationCmd
    {
        //屏幕控制实时   Screen     命令 Black黑屏  or Normal 正常
        //屏幕控制      ScreenScheduled    屏幕状态  正常显示  黑屏  
        //亮度调节实时          Brightness      
        //亮度调节          BrightnessScheduled

        //音量调节  Volume
        //对时配置  Time
        //重新启动  ReStart
        //发送文件  SyncFile 参数编辑好的节目所有信息

        /// <summary>
        /// 直接控制屏幕是否黑屏
        /// 参数： Black  黑屏 | Normal 正常
        /// </summary>
        public readonly static string CmdScreen = "CMD|Screen|";
        /// <summary>
        /// 定时控制屏幕是否黑屏
        /// 参数：ScreenControl 类
        /// </summary>
        public readonly static string CmdScreenScheduled = "CMD|ScreenScheduled|";

        /// <summary>
        /// 直接控制 亮度调节 
        /// 参数：0-100的数值
        /// </summary>
        public readonly static string CmdBrightness = "CMD|Brightness|";

        /// <summary>
        /// 定时控制 亮度调节 
        /// 参数：BrightnessControl 类
        /// </summary>
        public readonly static string CmdBrightnessScheduled = "CMD|BrightnessScheduled|";

        /// <summary>
        /// 直接控制 音量调节 
        /// 参数：0-100的数值
        /// </summary>
        public readonly static string CmdVolume = "CMD|Volume|";

        /// <summary>
        /// 直接控制 音量调节 
        /// 参数：VolumeControl 类
        /// </summary>
        public readonly static string CmdVolumeScheduled = "CMD|ReStart|";

        /// <summary>
        /// 直接控制 对时配置 
        /// 参数：UTF+8  时区信息    复杂的NTP信息 射频信息 GPS信息 对时
        /// </summary>
        public readonly static string CmdTime = "CMD|Time|";

        /// <summary>
        /// 直接控制 对时配置 
        /// 参数：UTF+8  时区信息    复杂的NTP信息 射频信息 GPS信息 对时
        /// </summary>
        public readonly static string CmdSyncTime = "CMD|SyncTime|";

        /// <summary>
        /// 直接控制 对时配置 
        /// 参数：UTF+8  时区信息    GPS信息 对时
        /// </summary>
        public readonly static string CmdTimeGPS = "CMD|TimeGPS|";

        /// <summary>
        /// 直接控制 重新启动 
        /// 参数：1
        /// </summary>
        public readonly static string CmdReStart = "CMD|ReStart|";

        /// <summary>
        /// 直接控制 重新启动 
        /// 参数：RestartControl 类
        /// </summary>
        public readonly static string CmdReStartScheduled = "CMD|ReStart|";

        /// <summary>
        /// 直接控制 发送文件 
        /// 参数：编辑好的节目所有信息对应的文件路径 存放在FTP文件夹下的文件 文件路径要是FTP文件夹
        /// </summary>
        public readonly static string CmdSyncFile = "CMD|SyncFile|";

        /// <summary>
        /// 发送用户信息并验证，返回机顶盒上现有配置文件 
        /// 参数：用户的账号与密码
        /// </summary>
        public readonly static string CmdVerifyUser = "CMD|VerifyUser|";

        /// <summary>
        /// 发送用户信息并验证，返回机顶盒上现有配置文件 
        /// 参数：用户的账号与密码
        /// </summary>
        public readonly static string CmdVerifySnCode = "CMD|VerifySnCode|";

        /// <summary>
        /// 发送用户信息并验证，返回机顶盒上现有配置文件 
        /// 参数：用户的账号与密码
        /// </summary>
        public readonly static string CmdSyncSnCode = "CMD|SyncSnCode|";

        /// <summary>
        /// 返回机顶盒上现有用户信息        
        /// </summary>
        public readonly static string CmdSyncUser = "CMD|SyncUser|";

        /// <summary>
        /// 返回机顶盒上设备控制记录        
        /// </summary>
        public readonly static string CmdSyncDeviceControl = "CMD|SyncDeviceControl|";

        /// <summary>
        /// 发送用户信息到机顶盒上
        /// </summary>
        public readonly static string CmdSendUser = "CMD|SendUser|";

        /// <summary>
        /// 发送节目基础信息到机顶盒上
        /// </summary>
        public readonly static string CmdSendProgram = "CMD|SendProgram|";

        /// <summary>
        /// 发送节目基础信息到机顶盒上
        /// </summary>
        public readonly static string CmdChangeProgram = "CMD|ChangeProgram|";

        /// <summary>
        ///从机顶盒上删除节目基础信息
        /// </summary>
        public readonly static string CmdDeleteProgram = "CMD|DeleteProgram|";

        /// <summary>
        /// 返回机顶盒上发布的节目列表        
        /// </summary>
        public readonly static string CmdSyncProgram = "CMD|SyncProgram|";

        /// <summary>
        ///从机顶盒上删除节目基础信息
        /// </summary>
        public readonly static string CmdEnableMonitor = "CMD|EnableMonitor|";

    }
}
