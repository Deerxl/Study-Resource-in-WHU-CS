using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace skin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {//选择文件
                bp_1 = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = bp_1;
            }
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
            IntPtr hWnd, // handle to destination window
            int Msg, // message
            int wParam, // first message parameter
            int lParam // second message parameter
        );

        private static MemoryStream ms_bmp_src;
        private static MemoryStream ms_bmp_result;

        private static IntPtr main_wnd_handle;
        private static Bitmap bp_1;
        public const int GRAY_FINISHED = 0x501;
        static void skin_mark()
        {
            //线程流程--图像相关
            if (bp_1 != null)
            {
                //准备位图１的字节数组
                ms_bmp_src.Seek(0, SeekOrigin.Begin);
                bp_1.Save(ms_bmp_src, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] buf_ms_src = ms_bmp_src.GetBuffer();
                //输出结果的内存区
                ms_bmp_result.Seek(0, SeekOrigin.Begin);
                bp_1.Save(ms_bmp_result, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] buf_ms_result = ms_bmp_result.GetBuffer();
                int src_width = bp_1.Width;//位图宽
                int src_height = bp_1.Height;//位图高
                int line_byte_count, scan_line_len;
                line_byte_count = src_width * 3;//24位需要处理成字节以4整倍数的填充行
                if ((line_byte_count % 4) == 0)
                {
                    scan_line_len = line_byte_count;
                }
                else
                {
                    scan_line_len = (line_byte_count / 4) * 4 + 4;
                }
                byte b_val, g_val, r_val;
                double H, S, I;
                double val_R, val_G, val_B, val_min, tem1, angle;
                double I_boarder = 100 / 255.0;
                //RGB-->HSI
                //
                for (int i_height = 0; i_height < src_height; i_height++)
                {
                    for (int i_width = 0; i_width < src_width; i_width++)
                    {
                        //
                        b_val = buf_ms_src[54 + i_height * scan_line_len + i_width * 3];
                        g_val = buf_ms_src[54 + i_height * scan_line_len + i_width * 3 + 1];
                        r_val = buf_ms_src[54 + i_height * scan_line_len + i_width * 3 + 2];
                        val_B = (double)(b_val / 255.0);
                        val_G = (double)(g_val / 255.0);
                        val_R = (double)(r_val / 255.0);
                        I = (val_R + val_G + val_B) / 3;
                        //Math.acos(double) 0≤θ≤π其中-1≤d≤1
                        //H=Math.acos(double)B<=GH=2π-Math.acos(double)B>G H 0 1.6 5.6 2pi
                        tem1 = ((val_R - val_G) + (val_R - val_B)) / 2 * (Math.Sqrt((val_R - val_G) * (val_R - val_G) + (val_R - val_G) * (val_G - val_B))); ;
                        //angle =Math.Acos(tem1);
                        //if (b_val <= g_val)
                        //{
                        // H = angle;
                        //}
                        //else {
                        // angle=2*Math.PI-angle;
                        // H
                        //}
                        //I = (R + G + B) / 3; I >100/255
                        val_min = Math.Min(Math.Min(val_B, val_G), val_R);
                        S = 1 - val_min * 3 / (val_R + val_G + val_B);
                        //S=1-((min(R,G,B))*3/(R+G+B)) 0.1<S<0.88
                        //R>240
                        if ((I > I_boarder) && (S < 0.88) && (S > 0.1) && (r_val > 200)
                        && (tem1 > Math.Cos(1.8)) && (tem1 <= 1.0))
                        {
                            //得出结果后设置像素值，这是皮肤像素
                        }
                        else
                        {
                            buf_ms_result[54 + i_height * scan_line_len + i_width * 3] = (byte)0;
                            buf_ms_result[54 + i_height * scan_line_len + i_width * 3 + 1] = (byte)0;
                            buf_ms_result[54 + i_height * scan_line_len + i_width * 3 + 2] = (byte)0;
                        }
                    }
                }//根据HSI模型运算每个像素是否是肤色
            }
            SendMessage(main_wnd_handle, GRAY_FINISHED, 100, 100);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread workThread = new Thread(new ThreadStart(skin_mark));
            workThread.IsBackground = true;
            workThread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            main_wnd_handle = this.Handle;
            ms_bmp_src = new MemoryStream(5000000);
            ms_bmp_result = new MemoryStream(5000000);
        }

        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case GRAY_FINISHED:
                    pictureBox2.Image = (Bitmap)Bitmap.FromStream(ms_bmp_result);
                    this.Invalidate();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

    }
}
