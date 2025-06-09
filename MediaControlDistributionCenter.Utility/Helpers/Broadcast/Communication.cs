using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Helpers.SocketClient;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Text;

namespace MediaControlDistributionCenter.Helpers.Broadcast
{
    public class Communication
    {
        //启动socket 链接
        System.Timers.Timer _heartbeatTimer;

        /// <summary>
        /// 接收到的命令列表
        /// </summary>
        public List<string> ReceiveOverCmdStr { get; private set; }

        public string SyncUserResult { get; private set; }

        public string SyncDeviceControlResult { get; private set; }

        public string SyncProgramResult { get; private set; }

        public string VerifySnCodeResult { get; private set; }

        public string SyncSnCodeResult { get; private set; }

        public string SyncTimeResult { get; private set; }

        public string SyncBrightnessResult { get; private set; }

        public string SyncVolumeResult { get; private set; }

        public string SyncFileProgressResult { get; private set; }

        public bool IsInternet { get; private set; }

        public FtpServer FtpServer => this.ftpServer;

        //本机及播控盒心跳数据
        public SocketHeart Heart = new SocketHeart();
        public NetClient netClient = new NetClient(false); //链接信息
        public string IpAddr; //Ip地址
        public string Port; //端口

        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;
        private int retryCount = 0;

        private readonly FtpServer ftpServer;

        public Communication(FtpServer ftpServer, bool isInternet = false)
        {
            this.ftpServer = ftpServer;
            this.IsInternet = isInternet;
            Heart.FtpPort = ftpServer._port;
            Heart.FtpUserName = ftpServer._userName;
            Heart.FtpUserPwd = ftpServer._userPwd;
            Heart.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            netClient.ErrorReceived += NetClient_ErrorReceived;
            netClient.Traced += NetClient_Traced;
            ReceiveOverCmdStr = new List<string>();
        }

        private void NetClient_Traced(object? sender, NetSockTracedInfoEventArgs e)
        {
            Log.Information($"Socket Traced: {e.TraceName}, Message: {e.Message}");
        }

        private void NetClient_ErrorReceived(object? sender, NetSockErrorReceivedEventArgs e)
        {
            Log.Error($"Socket Error: {e.Function}, Error Message: {e.Exception?.Message}");
        }

        /// <summary>
        /// 定时心跳包 保持长连接
        /// </summary>
        public void StartHeart()
        {
            //开启链接
            // 设置心跳包发送的间隔（例如，每5秒发送一次）
            int interval = 5000; // 5000毫秒即5秒 
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
            Heart.FtpIp = ftpServer._Ip;
            string HeartStr = JsonConvert.SerializeObject(Heart, Newtonsoft.Json.Formatting.Indented);
            string path = "Heart|Client|" + HeartStr;
            if (!path.EndsWith("##End##"))
            {
                path = string.Concat(path, "##End##");
            }

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(path);

            if (netClient.state == SocketState.Connected)
            {
                netClient.Send(utf8Bytes);
                retryCount = 0;
            }
            else
            {
                Log.Information($"Socket connection disconnected, need to reconnect!");
                //if (retryCount > 5)
                //{
                //    Disconnect();
                //}
                retryCount++;
                Thread thread = new Thread(() =>
                {
                    string HeartStr = JsonConvert.SerializeObject(Heart, Newtonsoft.Json.Formatting.Indented);
                    string path = "Heart|Client|" + HeartStr;
                    if (!path.EndsWith("##End##"))
                    {
                        path = string.Concat(path, "##End##");
                    }
                    byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(path);
                    IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(IpAddr), int.Parse(Port));
                    netClient.Connect(iPEnd);

                    Log.Information($"Socket connection reconnected!");
                    Log.Information($"Send Heart:{path}!");
                    if (netClient.state == SocketState.Connected)
                    {
                        netClient.Send(utf8Bytes);
                    }
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
            netClient.ReceiveCompleted += NetClient_ReceiveCompleted;
        }

        public void StartFtpServer()
        {
            if (!ftpServer.IsStarted)
            {
                Log.Information($"Connecting Ftp server IP:{ftpServer._Ip}, Port: {ftpServer._port}");
                ftpServer.FtpServerStart(IsInternet);
            }
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
                        if (data.Length == 3)
                        {
                            if (data[1].Contains(CommunicationCmd.CmdSyncUser.Split("|")[1]))
                            {
                                SyncUserResult = data[2];
                                Log.Information(SyncUserResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncDeviceControl.Split("|")[1]))
                            {
                                SyncDeviceControlResult = data[2];
                                Log.Information(SyncDeviceControlResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncProgram.Split("|")[1]))
                            {
                                SyncProgramResult = data[2];
                                Log.Information(SyncProgramResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdVerifySnCode.Split("|")[1]))
                            {
                                VerifySnCodeResult = data[2];
                                Log.Information(VerifySnCodeResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncSnCode.Split("|")[1]))
                            {
                                SyncSnCodeResult = data[2];
                                Log.Information(SyncSnCodeResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncTime.Split("|")[1]))
                            {
                                SyncTimeResult = data[2];
                                Log.Information(SyncTimeResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncBrightness.Split("|")[1]))
                            {
                                SyncBrightnessResult = data[2];
                                Log.Information(SyncBrightnessResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncVolume.Split("|")[1]))
                            {
                                SyncVolumeResult = data[2];
                                Log.Information(SyncVolumeResult);
                            }

                            if (data[1].Contains(CommunicationCmd.CmdSyncFile.Split("|")[1]))
                            {
                                SyncFileProgressResult = data[2];
                                Log.Information(SyncFileProgressResult);
                                if ((double.TryParse(SyncFileProgressResult, out var progress) && progress <= 100))
                                {
                                    InvokeProgressChanged?.Invoke(this, new ProgressEventArgs(progress));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
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
                        Log.Error(ex.Message);
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
            if (!Cmd.EndsWith("##End##"))
            {
                Cmd = string.Concat(Cmd, "##End##");
            }
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
            Log.Information($"Command: {Cmd} is starting to send!");
            string[] CmdArr = Cmd.Replace("\0", "").Split("|");
            string CmdOver = CmdArr[1] + "Over";

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(Cmd);
            netClient.Send(utf8Bytes);
            Log.Information($"Command: {Cmd} is sent!");

            if(CommunicationCmd.CmdSyncFile.Contains(CmdArr[1]))
            {
                SyncFileProgressResult = string.Empty;
            }

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

            Log.Information($"Task: {CmdArr[1]} is completed!");
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
