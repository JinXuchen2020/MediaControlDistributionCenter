using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Helpers.SocketClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaControlDistributionCenter.Helpers.Broadcast
{
    public class Communication
    {
        //启动socket 链接
        System.Timers.Timer _heartbeatTimer;

        /// <summary>
        /// 接收到的命令列表
        /// </summary>
        List<string> ReceiveOverCmdStr = new List<string>();

        public string SyncUserResult => "{\r\n    \"Users\": [\r\n        {\r\n            \"User\": {\r\n                \"account\": \"admin\",\r\n                \"company\": \"山木时代\",\r\n                \"contact\": \"12345678907\",\r\n                \"email\": \"1214@164.com\",\r\n                \"id\": 1,\r\n                \"password\": \"123456\",\r\n                \"role\": \"admin\",\r\n                \"status\": 1,\r\n                \"userGroupId\": 0\r\n            }\r\n        },\r\n        {\r\n            \"Monitor\": {\r\n                \"DeviceControls\": [\r\n                    {\r\n                        \"controlType\": \"Brightness\",\r\n                        \"deviceId\": \"24A\",\r\n                        \"execution\": \"00:00;00\",\r\n                        \"executionType\": \"REAL_TIME\",\r\n                        \"id\": 0,\r\n                        \"isEnabled\": 0,\r\n                        \"repeatMode\": \"\",\r\n                        \"userAccount\": \"user1\",\r\n                        \"validDateEnd\": \"2025/3/27\",\r\n                        \"validDateStart\": \"2025/3/27\",\r\n                        \"value\": 40\r\n                    }\r\n                ],\r\n                \"Monitor\": {\r\n                    \"contactName\": \"ss\",\r\n                    \"contactPhone\": \"156266\",\r\n                    \"deviceId\": \"24A\",\r\n                    \"enabled\": 1,\r\n                    \"height\": 768,\r\n                    \"id\": 1,\r\n                    \"name\": \"显示2\",\r\n                    \"snCode\": \"24A\",\r\n                    \"status\": \"在线\",\r\n                    \"userAccount\": \"user1\",\r\n                    \"validEnd\": \"2025-03-30 00:00:00\",\r\n                    \"validStart\": \"2025-03-27 00:00:00\",\r\n                    \"width\": 567\r\n                },\r\n                \"Programs\": [\r\n                    {\r\n                        \"createdSource\": \"会员\",\r\n                        \"id\": 3,\r\n                        \"isHasValidity\": false,\r\n                        \"lastUpdatedTime\": \"2025-03-27 20:08:00\",\r\n                        \"mediaType\": \"PROGRAM\",\r\n                        \"monitorCount\": 0,\r\n                        \"name\": \"节目名称20250327080808\",\r\n                        \"resolution\": \"256*192\",\r\n                        \"size\": 0.23,\r\n                        \"status\": 1,\r\n                        \"userAccount\": \"user1\"\r\n                    },\r\n                    {\r\n                        \"createdSource\": \"会员\",\r\n                        \"id\": 4,\r\n                        \"isHasValidity\": false,\r\n                        \"lastUpdatedTime\": \"2025-03-27 21:02\",\r\n                        \"mediaType\": \"PROGRAM\",\r\n                        \"monitorCount\": 1,\r\n                        \"name\": \"节目名称20250327085214\",\r\n                        \"resolution\": \"256*192\",\r\n                        \"size\": 0.24,\r\n                        \"status\": 1,\r\n                        \"userAccount\": \"user1\"\r\n                    },\r\n                    {\r\n                        \"createdSource\": \"管理员\",\r\n                        \"id\": 5,\r\n                        \"isHasValidity\": false,\r\n                        \"lastUpdatedTime\": \"2025-03-27 21:13\",\r\n                        \"mediaType\": \"PROGRAM\",\r\n                        \"monitorCount\": 0,\r\n                        \"name\": \"节目名称20250327091336\",\r\n                        \"resolution\": \"256*192\",\r\n                        \"size\": 0.1,\r\n                        \"status\": 1,\r\n                        \"userAccount\": \"user1\"\r\n                    },\r\n                    {\r\n                        \"createdSource\": \"会员\",\r\n                        \"id\": 6,\r\n                        \"isHasValidity\": false,\r\n                        \"lastUpdatedTime\": \"2025-03-27 21:18\",\r\n                        \"mediaType\": \"PROGRAM\",\r\n                        \"monitorCount\": 0,\r\n                        \"name\": \"节目名称20250327091742\",\r\n                        \"resolution\": \"768*576\",\r\n                        \"size\": 0.64,\r\n                        \"status\": 1,\r\n                        \"userAccount\": \"user1\"\r\n                    }\r\n                ]\r\n            },\r\n            \"User\": {\r\n                \"account\": \"user1\",\r\n                \"company\": \"shanm\",\r\n                \"id\": 2,\r\n                \"password\": \"123456\",\r\n                \"region\": \"shh\",\r\n                \"role\": \"user\",\r\n                \"status\": 0,\r\n                \"timeZone\": \"(UTC+08:00) 北京，重庆，香港特别行政区，乌鲁木齐\",\r\n                \"userGroupId\": 0\r\n            }\r\n        }\r\n    ]\r\n}";

        public string SyncDeviceControlResult { get; private set; }


        //本机及播控盒心跳数据
        public SocketHeart Heart = new SocketHeart();
        public NetClient netClient = new NetClient(false); //链接信息
        public string IpAddr; //Ip地址
        public string Port; //端口

        public Communication(FtpServer ftpServer)
        {
            Heart.FtpIp = ftpServer._Ip;
            Heart.FtpPort = ftpServer._port;
            Heart.FtpUserName = ftpServer._userName;
            Heart.FtpUserPwd = ftpServer._userPwd;
            Heart.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 定时心跳包 保持长连接
        /// </summary>
        public void StartHeart()
        {
            //开启链接
            // 设置心跳包发送的间隔（例如，每5秒发送一次）
            int interval = 1000; // 5000毫秒即5秒 
            // 创建定时器并设置间隔
            _heartbeatTimer = new System.Timers.Timer(interval);

            // 设置定时器的回调方法
            _heartbeatTimer.Elapsed += _heartbeatTimer_Elapsed;

            // 启动定时器
            _heartbeatTimer.Start();
        }
        /// <summary>
        /// 心跳处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _heartbeatTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            string HeartStr = JsonConvert.SerializeObject(Heart, Newtonsoft.Json.Formatting.Indented);
            string path = "Heart|Client|" + HeartStr;

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(path);

            if (netClient.state == SocketState.Connected)
            {
                netClient.Send(utf8Bytes);
            }
            else
            {
                Thread thread = new Thread(() =>
                {
                    IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(IpAddr), int.Parse(Port));
                    netClient.Connect(iPEnd);
                    netClient.Send(utf8Bytes);
                });

                thread.Start();
            }
        }
        /// <summary>
        /// 链接播控盒
        /// </summary>
        /// <param name="IpAddr"></param>
        /// <param name="Port"></param>
        public void Connect(string IpAddr, string Port)
        {
            this.IpAddr = IpAddr;
            this.Port = Port;
            IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(this.IpAddr), int.Parse(this.Port));
            netClient.Connect(iPEnd);
            netClient.ReceiveCompleted += NetClient_ReceiveCompleted; ;
        }        

        /// <summary>
        /// 断开与 播控盒链接
        /// </summary>
        public void Disconnect()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Stop();
                _heartbeatTimer.Dispose();
                netClient.Close("主动断开");
            }
        }
        /// <summary>
        /// 接收数据完成后的命令处理
        /// </summary>
        /// <param name="obj"></param>
        private void NetClient_ReceiveCompleted(byte[] obj)
        {
            string str = Encoding.UTF8.GetString(obj);
            string[] data = str.Replace("\0", "").Split("|", 3);
            switch (data[0])
            {
                case "CMD":
                    try
                    {
                        ReceiveOverCmdStr.Add(data[1]);
                        //if (data[1].Contains(CommunicationCmd.CmdSyncUser.Split("|")[1]))
                        //{
                        //    SyncUserResult = data[2];
                        //}

                        if (data[1].Contains(CommunicationCmd.CmdSyncDeviceControl.Split("|")[1]))
                        {
                            SyncDeviceControlResult = data[2];
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;

                case "Heart":
                    try
                    {
                        Heart = JsonConvert.DeserializeObject<SocketHeart>(data[2]);

                        string HeartStr = JsonConvert.SerializeObject(Heart, Newtonsoft.Json.Formatting.Indented);


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 执行命令异步
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="waitExecTime"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteCmdAsync(string Cmd, TimeSpan waitExecTime)
        {
            // 设置 
            using (var cancellationTokenSource = new CancellationTokenSource(waitExecTime))
            {
                try
                {
                    // 执行任务，传入取消令牌
                    bool completed = await Task.Run(() => SendCmd(cancellationTokenSource.Token, Cmd), cancellationTokenSource.Token);

                    // 如果工作完成，返回 true
                    return completed;
                }
                catch (OperationCanceledException)
                {
                    // 如果超时了，捕获取消异常并返回 false
                    return false;
                }
            }
        }


        /// <summary>
        /// 实际的发送命令
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="Cmd"></param>
        /// <returns></returns>
        bool SendCmd(CancellationToken cancellationToken, string Cmd)
        {
            // 获取等接收返回指令
            string[] CmdArr = Cmd.Replace("\0", "").Split("|");
            string CmdOver = CmdArr[1] + "Over";

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(Cmd);
            netClient.Send(utf8Bytes);

            while (true)
            {
                // 检查是否被取消
                if (cancellationToken.IsCancellationRequested || netClient.state != SocketState.Connected)
                {
                    return false;
                }

                if (ReceiveOverCmdStr.Contains(CmdOver))
                {
                    lock (this)
                    {

                        ReceiveOverCmdStr.Remove(CmdOver);
                        break;
                    }
                }
            }
            return true;  // 如果成功完成计算
        }

        //ExecuteCmdAsync 调用示例
        //string path = "CMD|Brightness|2";
        //bool t = await ExecuteWithTimeoutAsync(path, TimeSpan.FromMilliseconds(3000));
        //if (t)
        //{
        //    SendState.Text += "命令处理成功\r\n";
        //}
        //else
        //{
        //    SendState.Text += "命令无法被处理\r\n";
        //}
    }
}
