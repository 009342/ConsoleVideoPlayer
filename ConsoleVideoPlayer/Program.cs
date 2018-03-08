using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using FFmpeg4NET.VideoFileReader; //AForge.NET 인용
using System.Diagnostics;
using NAudio.Wave;

namespace ConsoleVideoPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            //video : music
            bool isSound = true;
            VideoFileReader vfr = new VideoFileReader();
            MediaFoundationReader video = null;
            try
            {
                video = new MediaFoundationReader(args.Length==2?args[1]:args[0]);
                Console.WriteLine("오디오 정보");
                Console.WriteLine("인코딩 : " + video.WaveFormat.ToString());
                Console.WriteLine("샘플레이트 : " + video.WaveFormat.SampleRate);
                Console.WriteLine("채널 : " + video.WaveFormat.Channels);
                Console.WriteLine("길이 :" + video.Length);
            }
            catch (Exception e)
            {
                isSound = false;
                Console.WriteLine("소리를 재생할 수 없습니다.");
                Console.WriteLine("오류 : " + e.ToString());
            }
            vfr.Open(args[0]);
            double fps = (double)vfr.FrameRateNum / (double)vfr.FrameRateDen;
            Console.WriteLine("비디오 정보");
            Console.WriteLine("코덱 정보 : " + vfr.CodecName);
            Console.WriteLine("초당 프레임 : " + fps.ToString());
            Console.WriteLine("총 프레임 : " + vfr.CurrentFrame);
            Console.WriteLine("너비 : " + vfr.Width);
            Console.WriteLine("높이 : " + vfr.Height);
            int Count = 5;
            while(Count-->0)
            {
                Console.WriteLine(Count.ToString());
                Thread.Sleep(1000);
            }
            Console.Clear();
            int c_width = 189;
            int c_height = 50;
            Console.SetWindowSize(
    Math.Min(c_width, Console.LargestWindowWidth),
    Math.Min(c_height, Console.LargestWindowHeight));
            int v_width = vfr.Width;
            int v_height = vfr.Height;
            int b_width = v_width / c_width;
            int b_height = v_height / c_height;
            Stopwatch s = new Stopwatch();
            WaveOut wo = new WaveOut();
            if (isSound) wo.Init(video);
            s.Start();
            if (isSound) wo.Play();
            Bitmap bm = new Bitmap(vfr.Width, vfr.Height);
            Random random = new Random();
            
            bool EOF = false;
            for (; ; )
            {
                while (vfr.CurrentFrame < (int)((s.Elapsed.TotalMilliseconds / 1000) * fps))
                {
                    if (vfr.CurrentFrame + 1 == vfr.FrameCount)
                    {
                        EOF = true;
                        break;
                    }
                    vfr.ReadVideoFrame().Dispose();
                }
                if (EOF) break;
                bm = vfr.ReadVideoFrame(); ;

                Console.Title = "현재 프레임 : " + vfr.CurrentFrame.ToString() + "시간 : " + ((s.Elapsed.TotalMilliseconds / 1000)).ToString() + "초당 프레임 : " + fps.ToString();
                Size resize = new Size(c_width, c_height);
                Bitmap resized = new Bitmap(bm, resize); //평균값 구하는거 시간도 걸리고 귀찮으니 걍 리사이즈 해버립시다.
                if (isSound) wo.Play();
                //Console.Title = "4";
                for (int l = 0; l < c_height; l++)//
                {
                    for (int c = 0; c < c_width; c++)
                    {
                        //Console.BackgroundColor = ClosestConsoleColor(AverageColor(bm, c, l, b_width, b_height));
                        Console.BackgroundColor = ClosestConsoleColor(resized.GetPixel(c, l));
                        Console.Write(" ");
                    }
                    //Console.WriteLine();
                }
                //Console.Title = "5";
                Console.SetCursorPosition(0, 0);
                //Console.Title = "6";
                bm.Dispose();
                resized.Dispose();
            }
            vfr.Close();
            if (isSound) video.Close();
        }
        /*static Color AverageColor(Bitmap bm, int col, int lin, int b_width, int b_height)
        {
            int R = 0;
            int G = 0;
            int B = 0;
            int count = 0;
            for (int x = col * b_width; x < (col + 1) * b_width; x++)
            {
                for (int y = lin * b_height; y < (lin + 1) * b_height; y++)
                {
                    count++;
                    R += bm.GetPixel(x, y).R;
                    G += bm.GetPixel(x, y).G;
                    B += bm.GetPixel(x, y).B;
                }
            }
            Color c = Color.FromArgb(R / count, G / count, B / count);
            return c;
        }*/

        static ConsoleColor ClosestConsoleColor(Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);

                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Olive" : n);
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
