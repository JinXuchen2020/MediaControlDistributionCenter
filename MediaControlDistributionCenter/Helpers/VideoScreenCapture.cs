using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaControlDistributionCenter.Helpers
{
    public class VideoScreenCapture
    {
        public static void CaptureFrame(string videoFilePath, string outputFilePath, int frameNumber)
        {
            // 打开视频文件
            using (var capture = new VideoCapture(videoFilePath))
            {
                if (!capture.IsOpened())
                {
                    MessageBox.Show("无法打开视频文件！");
                    return;
                }

                // 获取视频的总帧数
                int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);

                // 设置要截取的帧号（例如第100帧）
                capture.Set(VideoCaptureProperties.PosFrames, frameNumber);

                // 读取帧
                Mat frame = new Mat();
                capture.Read(frame);

                if (frame.Empty())
                {
                    MessageBox.Show("无法读取帧！");
                    return;
                }

                // 保存帧为图片
                Cv2.ImWrite(outputFilePath, frame);
            }
        }
    }
}
