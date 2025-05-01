using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Broadcast.Entity
{
    public class SocketHeart
    {
        //桌面端维护
        public string FtpIp { get; set; } //同步文件服务IP
        public int FtpPort { get; set; }//同步文件服务端口
        public string FtpUserName { get; set; }//同步文件服务用户名
        public string FtpUserPwd { get; set; } //同步文件服务密码
        public string Time { get; set; } //用于记录数据的时间

        //平板端维护
        public string DrivceName { get; set; } //设备名称
        public int Volume { get; set; }  //音量调节
        public int Brightness { get; set; }    //设备亮度
        public int DeviceCapacity { get; set; }//机器容量
        public bool IsSyncFile { get; set; } //正在同步文件
    }
}
