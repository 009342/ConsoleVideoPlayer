using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Video;
using Accord.Video.FFMPEG;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using NAudio;
using NAudio.Wave;

namespace ConsoleVideoPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            VideoFileReader vfr = new VideoFileReader();
            MediaFoundationReader video = new MediaFoundationReader(args[0]);
            vfr.Open(args[0]);
            int c_width = 120;
            int c_height = 30;
            int v_width = vfr.Width;
            int v_height = vfr.Height;
            int b_width = v_width / c_width;
            int b_height = v_height / c_height;
            Stopwatch s = new Stopwatch();
            WaveOut wo = new WaveOut();
            wo.Init(video);
            s.Start();
            wo.Play();
            for (;;)
            {
                int frame = (int)(((float)s.ElapsedMilliseconds / 1000f) * (vfr.FrameRate.Numerator / vfr.FrameRate.Denominator));
                if (frame > vfr.FrameCount) break;
                Bitmap bm = vfr.ReadVideoFrame(frame);
                for (int l = 0; l < c_height; l++)//
                {
                    for (int c = 0; c < c_width; c++)
                    {
                        Console.BackgroundColor = ClosestConsoleColor(AverageColor(bm, c, l, b_width, b_height));
                        Console.Write(" ");
                    }
                }
                Console.SetCursorPosition(0, 0);
                bm = null;
            }
        }
        static Color AverageColor(Bitmap bm, int col, int lin, int b_width, int b_height)
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
        }
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
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
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
