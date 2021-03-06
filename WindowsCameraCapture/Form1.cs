﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using BeautoRover;

namespace WindowsCameraCapture
{
    public partial class Form1 : Form
    {
        BeautoRoverlib br = new BeautoRoverlib();
        private CvCapture capture;

        public object COLOR_BGR2HSV { get; private set; }

        public Form1()
        {
            InitializeComponent();
            capture = CvCapture.FromCamera(0);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Setting framewidth and FrameHeight. In this case, framewidht is 320, and frameheight is 240.
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameWidth, 320);
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameHeight, 240);
            textBox1.Text = br.OpenCOMPort("COM7");

            // タイマーをスタート. Timer starts.
            timer1.Start();
            // 停止ボタンを使えるようにする. Enable stop button.
            buttonStop.Enabled = true;
            // 開始ボタンを使えないようにする Disable start button.
            buttonStart.Enabled = false;
        }
        //private int ConvertToBmp(CvColor center)
        //{
        //    if (center.R < 70 && center.B < 70 && center.G > 80)
        //    {
        //        return 1;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}
       
        private int detect(IplImage ipl2, int a ,int b)
        {
            int sum = 0;
            for (int y = 0; y < ipl2.Height; y++)
            {
                for (int x = ipl2.Width/7*a; x < ipl2.Width / 7 * b; x++)
                {
                    CvColor c = ipl2[y, x];

                    // Red color extraction
                    // If the pixel is red-like, the image is white, else black.
                    if (c.R < 30 && c.G < 30 && c.B > 50)
                    {
                        sum++;
                    }
                }
            }
            return (sum-40>0?1:0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // キャプチャの開始. Capture starts.
            IplImage ipl1 = capture.QueryFrame();
            //Cv.CvtColor(ipl1, ipl2, ColorConversion.BgrToHsv);
            // 取得したカメラ画像の高さと幅を取得し、labelに表示. Height and width of camera are shown in label.
            labelWidth.Text = capture.FrameWidth.ToString();
            labelHeight.Text = capture.FrameHeight.ToString();

            if (ipl1 != null)
            {
                // pictureBoxに取得した画像を表示. Show the captured image.
                pictureBox1.Image = ipl1.ToBitmap();
                // メモリリークが発生するらしいので
                // プログラムが動的に確保したメモリ領域のうち、
                // 不要になった領域を定期的に自動解放する
                if (GC.GetTotalMemory(false) > 600000)
                {
                    GC.Collect();
                }

                // Image processing should be written from here.

                // Extract red color

                CvColor center = ipl1[120, 160];
                CvColor left =  ipl1[120, 0];
                CvColor right = ipl1[119, 319];
                int centerInt = detect(ipl1,1,6);
                int leftInt = detect(ipl1,0,1);
                int rightInt = detect(ipl1,6,7);

                for (int y = 0; y < ipl1.Height; y++)
                {
                    for (int x = 0; x < ipl1.Width; x++)
                    {
                        CvColor c = ipl1[y, x];
                        // Red color extraction
                        // If the pixel is red-like, the image is white, else black.
                        if (c.R < 30 && c.G < 30 && c.B > 50)
                        {
                            ipl1[y, x] = new CvColor()
                            {
                                B = 255,
                                G = 255,
                                R = 255,
                            };
                        }
                        else
                        {
                            ipl1[y, x] = new CvColor()
                            {
                                // Red color extraction
                                B = 0,
                                G = 0,
                                R = 0,
                            };

                        }
                    }
                }
                // Show the image to picturebox2.
                pictureBox2.Image = ipl1.ToBitmap();
                //control the car

                if (centerInt == 1 && leftInt == 0 && rightInt == 0)
                {
                    textBox1.Text = br.Forward();
                } 
                else if (leftInt == 1 && rightInt==0)
                {
                    textBox1.Text = br.TurnLeft();
                }
                else if(rightInt==1 && leftInt==0)
                {
                    textBox1.Text = br.TurnRight();
                }
                else if (leftInt == 1 && rightInt == 1)
                {
                    textBox1.Text = br.Back();
                }
                else
                {
                    textBox1.Text = br.Stop();
                }

            }
            else
            {
                timer1.Stop();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            // タイマーをストップ. Timer stop.
            timer1.Stop();
            // 開始ボタンを使えるようにする. Enable start button.
            buttonStart.Enabled = true;
            // 停止ボタンを使えないようにする. Disable stop button.
            buttonStop.Enabled = false;
            textBox1.Text = br.Close();

        }
    }
}
