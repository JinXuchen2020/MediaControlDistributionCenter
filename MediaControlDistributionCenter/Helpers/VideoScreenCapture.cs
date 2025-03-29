using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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

        public static BitmapSource? CaptureFrame(string videoFilePath, int frameNumber)
        {
            // 打开视频文件
            using (var capture = new VideoCapture(videoFilePath))
            {
                if (!capture.IsOpened())
                {
                    MessageBox.Show("无法打开视频文件！");
                    return null;
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
                    return null;
                }

                // 保存帧为图片
                using var memory = frame.ToMemoryStream();
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memory;
                bitmap.EndInit();
                return bitmap;
            }
        }
    }
}
