using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        

        public static IntPtr main_wnd_handle;//主窗体句柄


        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );

        //定义消息常数 
        public const int TRAN_FINISHED = 0x500;

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {//选择文件
                DataClass.bp_1 = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = DataClass.bp_1;
            }       
        }
        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case TRAN_FINISHED:
                    label1.Text = "图片灰度处理完成";
                    pictureBox1.Image = DataClass.bp_grey;
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        public static long bmp_file_len;//文件长度变量
        //灰度计算的线程体 
        static void thread_bmp_2_grey()
        {
            //线程流程
            if (DataClass.bp_1 != null)
            {
                DataClass.ms_bmp.Seek(0, SeekOrigin.Begin);
                DataClass.bp_1.Save(DataClass.ms_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                bmp_file_len = DataClass.ms_bmp.Position;
                //RGB2GRAY(r,g,b) (((b)*117 + (g)*601 + (r)*306) >> 10)
                byte[] buf_ms = DataClass.ms_bmp.GetBuffer();

                byte r_1, g_1, b_1, grey_val;
                int scan_line_len;
                switch (DataClass.bp_1.PixelFormat)
                {
                    case PixelFormat.Format32bppRgb:
                        {//32位真彩色 排列方式BGRA 
                            scan_line_len = DataClass.bp_1.Width * 4;
                            for (int i_height = 0; i_height < DataClass.bp_1.Height; i_height++)
                            {//for each line
                                for (int i_width = 0; i_width < DataClass.bp_1.Width; i_width++)
                                {
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4];
                                    g_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4 + 1];
                                    r_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4 + 2];
                                    grey_val = (byte)(((b_1) * 117 + (g_1) * 601 + (r_1) * 306) >> 10);
                                    buf_ms[54 + i_height * scan_line_len + i_width * 4] = grey_val;
                                    buf_ms[54 + i_height * scan_line_len + i_width * 4 + 1] = grey_val;
                                    buf_ms[54 + i_height * scan_line_len + i_width * 4 + 2] = grey_val;
                                }//each pixel in one line 
                            }
                            break;
                        }
                    case PixelFormat.Format24bppRgb:
                        {//24位真彩色
                            int i_bmp_width = DataClass.bp_1.Width;
                            int line_byte_count;
                            //line_byte_count=i_bmp_width*3;24位需要处理
                            line_byte_count = i_bmp_width * 3;//32位直接计算

                            if ((line_byte_count % 4) == 0)
                            {
                                scan_line_len = line_byte_count;
                            }
                            else
                            {
                                scan_line_len = (line_byte_count / 4) * 4 + 4;
                            }
                            //24位位图，因此每像素使用24字节，没有alpha字节 
                            for (int i_height = 0; i_height < DataClass.bp_1.Height; i_height++)
                            {//for each line
                                for (int i_width = 0; i_width < DataClass.bp_1.Width; i_width++)
                                {
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3];
                                    g_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3 + 1];
                                    r_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3 + 2];
                                    grey_val = (byte)(((b_1) * 117 + (g_1) * 601 + (r_1) * 306) >> 10);
                                    buf_ms[54 + i_height * scan_line_len + i_width * 3] = grey_val;
                                    buf_ms[54 + i_height * scan_line_len + i_width * 3 + 1] = grey_val;
                                    buf_ms[54 + i_height * scan_line_len + i_width * 3 + 2] = grey_val;
                                }//each pixel in one line 
                            }
                            break;
                        }
                }
                DataClass.bp_grey = (Bitmap)Bitmap.FromStream(DataClass.ms_bmp);
                SendMessage(main_wnd_handle, TRAN_FINISHED, 100, 100);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //启动灰度计算线程			 
            Thread workThread = new Thread(new ThreadStart(thread_bmp_2_grey));
            workThread.IsBackground = true;
            workThread.Start();    
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataClass.ms_bmp = new MemoryStream(5000000);
            DataClass.buf_red_hist = new Int32[256];//8bit
            DataClass.buf_green_hist = new Int32[256];//8bit
            DataClass.buf_blue_hist = new Int32[256];//8bit
            DataClass.buf_gray_hist = new Int32[256];//8bit
            main_wnd_handle = this.Handle;    
        }


        //直方图计算的线程体 
        static void thread_histgram()
        {
            //线程流程
            if (DataClass.bp_1 != null)
            {
                DataClass.ms_bmp.Seek(0, SeekOrigin.Begin);
                DataClass.bp_1.Save(DataClass.ms_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                bmp_file_len = DataClass.ms_bmp.Position; 
                byte[] buf_ms = DataClass.ms_bmp.GetBuffer(); 
                byte r_1, g_1, b_1, grey_val;
                int scan_line_len;
                switch (DataClass.bp_1.PixelFormat)
                {
                    case PixelFormat.Format32bppRgb:
                        {//32位真彩色 排列方式BGRA 
                            scan_line_len = DataClass.bp_1.Width * 4;
                            for (int i_height = 0; i_height < DataClass.bp_1.Height; i_height++)
                            {//for each line
                                for (int i_width = 0; i_width < DataClass.bp_1.Width; i_width++)
                                {//对每个像素计算直方图，根据每个像素的值，对相应下标的直方图数组累加。
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4];
                                    g_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4 + 1];
                                    r_1 = buf_ms[54 + i_height * scan_line_len + i_width * 4 + 2];
                                    grey_val = (byte)(((b_1) * 117 + (g_1) * 601 + (r_1) * 306) >> 10);
                                    DataClass.buf_red_hist[b_1]++;
                                    DataClass.buf_green_hist[g_1]++;
                                    DataClass.buf_blue_hist[r_1]++;
                                    DataClass.buf_gray_hist[grey_val]++;
                                     
                                }//each pixel in one line 
                            }
                            break;
                        }
                    case PixelFormat.Format24bppRgb:
                        {//24位真彩色
                            int i_bmp_width = DataClass.bp_1.Width;
                            int line_byte_count;
                            //line_byte_count=i_bmp_width*3;24位需要处理
                            line_byte_count = i_bmp_width * 3;//32位直接计算

                            if ((line_byte_count % 4) == 0)
                            {
                                scan_line_len = line_byte_count;
                            }
                            else
                            {
                                scan_line_len = (line_byte_count / 4) * 4 + 4;
                            }
                            //24位位图，因此每像素使用24字节，没有alpha字节 
                            for (int i_height = 0; i_height < DataClass.bp_1.Height; i_height++)
                            {//for each line
                                for (int i_width = 0; i_width < DataClass.bp_1.Width; i_width++)
                                {
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3];
                                    g_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3 + 1];
                                    r_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3 + 2];
                                    grey_val = (byte)(((b_1) * 117 + (g_1) * 601 + (r_1) * 306) >> 10);
                                    DataClass.buf_red_hist[b_1]++;
                                    DataClass.buf_green_hist[g_1]++;
                                    DataClass.buf_blue_hist[r_1]++;
                                    DataClass.buf_gray_hist[grey_val]++;
                                }//each pixel in one line 
                            }
                            break;
                        }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //启动直方图计算线程			 
            Thread workThread = new Thread(new ThreadStart(thread_histgram));
            workThread.IsBackground = true;
            workThread.Start();   
            Form2 frm2 = new Form2();
            frm2.Show();
        }

    }//end
}
