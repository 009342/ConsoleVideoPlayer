using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using FFmpeg4NET.VideoFileReader; //AForge.NET 인용
using System.Diagnostics;
using NAudio.Wave;
using TrueColorConsole;
namespace ConsoleVideoPlayer
{
    class Program
    {
        static int healthy = 30;
        static int size = 59;
        static AudioFileReader audioFileReader;
        static List<Bitmap> Frames = new List<Bitmap>();
        static void Main(string[] args)
        {
            if (!VTConsole.IsSupported)
            {
                return;
            }
            VTConsole.Enable();
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("https://github.com/009342/ConsoleVideoPlayer");
            if (args.Length == 0) return;
            //video : music
            bool isSound = true;
            VideoFileReader vfr = new VideoFileReader();
            WaveOut waveOut = new WaveOut();
            if (args.Length == 2) size = int.Parse(args[1]);
            audioFileReader = new AudioFileReader(args[0]);
            MediaFoundationReader video = null;
            try
            {

                Console.WriteLine("오디오 정보");
                Console.WriteLine("인코딩 : " + audioFileReader.WaveFormat.ToString());
                Console.WriteLine("샘플레이트 : " + audioFileReader.WaveFormat.SampleRate);
                Console.WriteLine("채널 : " + audioFileReader.WaveFormat.Channels);
                Console.WriteLine("길이 :" + audioFileReader.Length);
            }
            catch (Exception e)
            {
                isSound = false;
                Console.WriteLine("소리를 재생할 수 없습니다.");
                Console.WriteLine("오류 : " + e.ToString());
            }
            vfr.Open(args[0]);
            double fps = (double)vfr.FrameRateNum / (double)vfr.FrameRateDen;
            fps = (fps > 59) ? 30 : fps;
            Console.WriteLine("비디오 정보");
            Console.WriteLine("코덱 정보 : " + vfr.CodecName);
            Console.WriteLine("초당 프레임 : " + fps.ToString());
            Console.WriteLine("총 프레임 : " + vfr.FrameCount);
            Console.WriteLine("너비 : " + vfr.Width);
            Console.WriteLine("높이 : " + vfr.Height);
            int Count = 5;
            while (Count-- > 0)
            {
                Console.WriteLine(Count.ToString());
                Thread.Sleep(1000);
            }
            Console.Clear();
            int c_width = 189;
            int c_height = 50;
            Console.SetWindowSize(
    Math.Min(c_width, Console.LargestWindowWidth),
    Console.LargestWindowHeight);
            int v_width = vfr.Width;
            int v_height = vfr.Height;
            int b_width = v_width / c_width;
            int b_height = v_height / c_height;

            if (isSound) waveOut.Init(audioFileReader);
            if (isSound) waveOut.Play();
            Bitmap bm = new Bitmap(vfr.Width, vfr.Height);
            Random random = new Random();
            Thread videoDecoderThread = new Thread(() => { VideoDecoderThread(vfr, fps); });
            Thread frameDisposerThread = new Thread(() => { FrameDisposerThread(vfr); });
            videoDecoderThread.Start();
            frameDisposerThread.Start();
            bool EOF = false;
            int tempIndex = 0;
            for (; ; )
            {

                while (Frames.Count < (int)((audioFileReader.CurrentTime.TotalMilliseconds / 1000) * fps) + healthy)
                {
                    if (vfr.CurrentFrame + 1 == vfr.FrameCount)
                    {
                        EOF = true;
                        break;
                    }
                    Thread.Sleep(10);
                    Console.Title = "패스 : " + vfr.CurrentFrame.ToString() + "/" + ((audioFileReader.CurrentTime.TotalMilliseconds / 1000) * fps).ToString();
                }
                if (EOF) break;
                tempIndex = (int)((audioFileReader.CurrentTime.TotalMilliseconds / 1000) * fps);
                bm = Frames[tempIndex];
                if (bm == null) break;
                Console.Title = "현재 프레임 : " + vfr.CurrentFrame.ToString() + " 제거된 프레임 : " + disposedFrame + " 시간 : " + ((audioFileReader.CurrentTime.TotalMilliseconds / 1000)).ToString() + "초당 프레임 : " + fps.ToString();
                #region deletedCodes
                /*
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
                */
                #endregion
                Console.SetCursorPosition(0, 0);
                //Console.Title = "콘솔 출력중...";

                ConsoleWriteImage2(bm);
                //Console.Title = "콘솔 출력 완료...";

                SetCurrentFrame(tempIndex);

            }
            vfr.Close();
            if (isSound) video.Close();
            videoDecoderThread.Abort();
            frameDisposerThread.Abort();
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
        #region otherCodes
        static int[] cColors = { 0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0, 0x808080, 0x0000FF, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0xFFFF00, 0xFFFFFF };

        public static void ConsoleWritePixel(Color cValue)
        {
            Color[] cTable = cColors.Select(x => Color.FromArgb(x)).ToArray();
            char[] rList = new char[] { '░', ' ', '▓', '█' }; // 1/4, 2/4, 3/4, 4/4
            int[] bestHit = new int[] { 0, 0, 4, int.MaxValue }; //ForeColor, BackColor, Symbol, Score

            for (int rChar = rList.Length; rChar > 0; rChar--)
            {
                for (int cFore = 0; cFore < cTable.Length; cFore++)
                {
                    for (int cBack = 0; cBack < cTable.Length; cBack++)
                    {
                        int R = (cTable[cFore].R * rChar + cTable[cBack].R * (rList.Length - rChar)) / rList.Length;
                        int G = (cTable[cFore].G * rChar + cTable[cBack].G * (rList.Length - rChar)) / rList.Length;
                        int B = (cTable[cFore].B * rChar + cTable[cBack].B * (rList.Length - rChar)) / rList.Length;
                        int iScore = (cValue.R - R) * (cValue.R - R) + (cValue.G - G) * (cValue.G - G) + (cValue.B - B) * (cValue.B - B);
                        if (!(rChar > 1 && rChar < 4 && iScore > 50000)) // rule out too weird combinations
                        {
                            if (iScore < bestHit[3])
                            {
                                bestHit[3] = iScore; //Score
                                bestHit[0] = cFore;  //ForeColor
                                bestHit[1] = cBack;  //BackColor
                                bestHit[2] = rChar;  //Symbol
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = (ConsoleColor)bestHit[0];
            Console.BackgroundColor = (ConsoleColor)bestHit[1];
            Console.Write(rList[bestHit[2] - 1]);
        }


        public static void ConsoleWriteImage(Bitmap source)
        {
            int sMax = 89;
            decimal percent = Math.Min(decimal.Divide(sMax, source.Width), decimal.Divide(sMax, source.Height));
            Size dSize = new Size((int)(source.Width * percent), (int)(source.Height * percent));
            Bitmap bmpMax = new Bitmap(source, dSize.Width * 2, dSize.Height);
            for (int i = 0; i < dSize.Height; i++)
            {
                for (int j = 0; j < dSize.Width; j++)
                {
                    ConsoleWritePixel(bmpMax.GetPixel(j * 2, i));
                    ConsoleWritePixel(bmpMax.GetPixel(j * 2 + 1, i));
                }
                System.Console.WriteLine();
            }
            Console.ResetColor();
        }
        public static void ConsoleWriteImage2(Bitmap bmpSrc)
        {
            int sMax = size;
            decimal percent = Math.Min(decimal.Divide(sMax, bmpSrc.Width), decimal.Divide(sMax, bmpSrc.Height));
            Size resSize = new Size((int)(bmpSrc.Width * percent), (int)(bmpSrc.Height * percent));
            /*Func<System.Drawing.Color, int> ToConsoleColor = c =>
            {
                int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                index |= (c.R > 64) ? 4 : 0;
                index |= (c.G > 64) ? 2 : 0;
                index |= (c.B > 64) ? 1 : 0;
                return index;
            };*/
            Bitmap bmpMin = new Bitmap(bmpSrc, resSize.Width, resSize.Height);
            Bitmap bmpMax = new Bitmap(bmpSrc, resSize.Width * 2, resSize.Height * 2);
#if FALSE
                        for (int i = 0; i < resSize.Height; i++)
            {
                /*for (int j = 0; j < resSize.Width; j++)
                {
                    Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMin.GetPixel(j, i));
                    Console.Write("██");
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("    ");
                */
                for (int j = 0; j < resSize.Width; j++)
                {
                    //Console.ForegroundColor = ClosestConsoleColor(bmpMax.GetPixel(j * 2, i * 2));
                    //Console.BackgroundColor = ClosestConsoleColor(bmpMax.GetPixel(j * 2, i * 2 + 1));
                    //Console.Write("▀");
                    VTConsole.Write("▀", bmpMax.GetPixel(j * 2, i * 2), bmpMax.GetPixel(j * 2, i * 2 + 1));
                    //Console.ForegroundColor = ClosestConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2));
                    //Console.BackgroundColor = ClosestConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2 + 1));
                    //Console.Write("▀");
                    VTConsole.Write("▀", bmpMax.GetPixel(j * 2 + 1, i * 2), bmpMax.GetPixel(j * 2 + 1, i * 2 + 1));
                }
                System.Console.WriteLine();
            }
#endif


            var builder = new StringBuilder((resSize.Width + 1) * resSize.Height);

            for (int i = 0; i < resSize.Height; i++)
            {
                string str = "";
                for (int j = 0; j < resSize.Width; j++)
                {
                    str += VTConsole.GetColorForegroundString(bmpMax.GetPixel(j * 2, i * 2).R, bmpMax.GetPixel(j * 2, i * 2).G, bmpMax.GetPixel(j * 2, i * 2).B);
                    str += VTConsole.GetColorBackgroundString(bmpMax.GetPixel(j * 2, i * 2 + 1).R, bmpMax.GetPixel(j * 2, i * 2 + 1).G, bmpMax.GetPixel(j * 2, i * 2 + 1).B);
                    str += "▀";
                    str += VTConsole.GetColorForegroundString(bmpMax.GetPixel(j * 2 + 1, i * 2).R, bmpMax.GetPixel(j * 2 + 1, i * 2).G, bmpMax.GetPixel(j * 2 + 1, i * 2).B);
                    str += VTConsole.GetColorBackgroundString(bmpMax.GetPixel(j * 2 + 1, i * 2 + 1).R, bmpMax.GetPixel(j * 2 + 1, i * 2 + 1).G, bmpMax.GetPixel(j * 2 + 1, i * 2 + 1).B);
                    str += "▀";


                }
                str += Environment.NewLine;
                builder.Append(str);
            }
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            VTConsole.WriteFast(bytes);
        }

        #endregion
        public static void VideoDecoderThread(VideoFileReader vfr, double fps)
        {
            while (true)
            {
                if (vfr.CurrentFrame < fps * (audioFileReader.CurrentTime.TotalMilliseconds / 1000) + healthy * 2)
                {
                    Frames.Add(vfr.ReadVideoFrame());
                }
                else
                {
                    Thread.Sleep(500);
                }
            }

        }
        static int currentFrame = 0;
        static int disposedFrame = 0;
        public static void SetCurrentFrame(int frame)
        {
            currentFrame = frame;
        }
        public static void FrameDisposerThread(VideoFileReader vfr)
        {
            while (true)
            {
                if (currentFrame < Frames.Count && disposedFrame < currentFrame)
                {
                    if (disposedFrame == vfr.FrameCount) break;
                    Frames[disposedFrame++].Dispose();

                }
                else
                {
                    Thread.Sleep(10);
                }
            }

        }
    }
}
