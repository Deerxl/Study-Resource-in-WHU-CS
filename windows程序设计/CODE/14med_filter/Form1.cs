using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections;

namespace med_filter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );
        //8位灰度图统计滤波的线程体 
        static void mid_filter()
        { 
            //线程流程--3*3中值滤波
            if (GData.bp_1 != null)
            {
                GData.ms_bmp.Seek(0, SeekOrigin.Begin);
                GData.bp_1.Save(GData.ms_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                GData.ms_bmp1.Seek(0, SeekOrigin.Begin);
                GData.bp_1.Save(GData.ms_bmp1, System.Drawing.Imaging.ImageFormat.Bmp);

                byte[] buf_ms = GData.ms_bmp.GetBuffer();
                byte[] buf_ms1 = GData.ms_bmp1.GetBuffer();
                byte b_1, grey_val;
                ArrayList sta_ar = new ArrayList();
                int scan_line_len;
                switch (GData.bp_1.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        {//24位真彩色
                            int i_bmp_width = GData.bp_1.Width;
                            int line_byte_count;
                            //line_byte_count=i_bmp_width*3;
                            line_byte_count = i_bmp_width * 3;//24位需要处理32位直接计算

                            if ((line_byte_count % 4) == 0)
                            {
                                scan_line_len = line_byte_count;
                            }
                            else
                            {
                                scan_line_len = (line_byte_count / 4) * 4 + 4;
                            }
                            //24位位图，因此每像素使用24字节，没有alpha字节，灰度图是R=G=B
                            for (int i_height = 1; i_height < GData.bp_1.Height - 1; i_height++)
                            {//for each line
                                for (int i_width = 1; i_width < GData.bp_1.Width - 1; i_width++)
                                {
                                    sta_ar.Clear();//清空集合
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3];
                                    sta_ar.Add(b_1);//中心点
                                    b_1 = buf_ms[54 + (i_height) * scan_line_len + (i_width - 1) * 3];
                                    sta_ar.Add(b_1);//x-1
                                    b_1 = buf_ms[54 + (i_height) * scan_line_len + (i_width + 1) * 3];
                                    sta_ar.Add(b_1);//x+1

                                    b_1 = buf_ms[54 + (i_height - 1) * scan_line_len + i_width * 3];
                                    sta_ar.Add(b_1);//y-1
                                    b_1 = buf_ms[54 + (i_height - 1) * scan_line_len + (i_width - 1) * 3];
                                    sta_ar.Add(b_1);//y-1,x-1
                                    b_1 = buf_ms[54 + (i_height - 1) * scan_line_len + (i_width + 1) * 3];
                                    sta_ar.Add(b_1);//y-1,x+1


                                    b_1 = buf_ms[54 + (i_height + 1) * scan_line_len + i_width * 3];
                                    sta_ar.Add(b_1);//y+1
                                    b_1 = buf_ms[54 + (i_height + 1) * scan_line_len + (i_width + 1) * 3];
                                    sta_ar.Add(b_1);//y+1,x+1
                                    b_1 = buf_ms[54 + (i_height + 1) * scan_line_len + (i_width - 1) * 3];
                                    sta_ar.Add(b_1);//y+1,x-1 
                                    sta_ar.Sort();
                                    grey_val = (byte)sta_ar[4];
                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3] = grey_val;
                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3 + 1] = grey_val;
                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3 + 2] = grey_val;
                                }//each pixel in one line 
                            }

                            break;
                        }
                }
            }
            SendMessage(GData.frm2_wnd_handle, GData.GRAY_FINISHED, 100, 100);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Thread workThread = new Thread(new ThreadStart(mid_filter));
            workThread.IsBackground = true;
            workThread.Start();

            GData.frm2.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {//选择文件
                GData.bp_1 = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = GData.bp_1;
            }      
        }


        //8位灰度图拉普拉斯锐化的线程体 
        static void laplaceian_sharpen()
        {
            //线程流程--page101掩膜系数为a图
            if (GData.bp_1 != null)
            {
                GData.ms_bmp.Seek(0, SeekOrigin.Begin);
                GData.bp_1.Save(GData.ms_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                GData.ms_bmp1.Seek(0, SeekOrigin.Begin);
                GData.bp_1.Save(GData.ms_bmp1, System.Drawing.Imaging.ImageFormat.Bmp);

                byte[] buf_ms = GData.ms_bmp.GetBuffer();
                byte[] buf_ms1 = GData.ms_bmp1.GetBuffer();
                byte b_1, grey_val;
                int g_val=0,i_val=0;
                int scan_line_len;
                switch (GData.bp_1.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        {//24位真彩色
                            int i_bmp_width = GData.bp_1.Width;
                            int line_byte_count;
                            //line_byte_count=i_bmp_width*3;
                            line_byte_count = i_bmp_width * 3;//24位需要处理32位直接计算
                            //scan_line_len作为每行扫描线的字节数目。
                            if ((line_byte_count % 4) == 0)
                            {
                                scan_line_len = line_byte_count;
                            }
                            else
                            {
                                scan_line_len = (line_byte_count / 4) * 4 + 4;
                            }
                            int min_val = 255, max_val = 0;

                            //24位位图，因此每像素使用24字节，没有alpha字节，灰度图是R=G=B
                            for (int i_height = 1; i_height < GData.bp_1.Height - 1; i_height++)
                            {//for each line
                                for (int i_width = 1; i_width < GData.bp_1.Width - 1; i_width++)
                                {
                                    //g(x,y)=5f(x,y)-[f(x+1,y)+f(x-1,y)+f(x,y+1)f(x,y-1)]
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3];
                                    //中心点
                                    i_val = (int)b_1;
                                    g_val = i_val * 5; 

                                    b_1 = buf_ms[54 + (i_height - 1) * scan_line_len + i_width * 3];
                                    //y-1
                                    i_val = (int)b_1;
                                    g_val = g_val - i_val;

                                    b_1 = buf_ms[54 + (i_height + 1) * scan_line_len + i_width * 3];
                                    i_val = (int)b_1;
                                    //y+1
                                    g_val = g_val - i_val; 

                                    b_1 = buf_ms[54 + i_height  * scan_line_len + (i_width+1) * 3];
                                    i_val = (int)b_1;
                                    //x+1
                                    g_val = g_val - i_val;

                                    b_1 = buf_ms[54 + i_height * scan_line_len + (i_width -1) * 3];
                                    i_val = (int)b_1;
                                    //x-1
                                    g_val = g_val - i_val;

                                    if (g_val<min_val)
                                    {
                                        min_val = g_val;
                                    }
                                    if (g_val > max_val)
                                    {
                                        max_val = g_val;
                                    } 
                                }//each pixel in one line 
                            }

                            int most_val = max_val - min_val;


                            for (int i_height = 1; i_height < GData.bp_1.Height - 1; i_height++)
                            {//for each line
                                for (int i_width = 1; i_width < GData.bp_1.Width - 1; i_width++)
                                {
                                    //g(x,y)=5f(x,y)-[f(x+1,y)+f(x-1,y)+f(x,y+1)f(x,y-1)]
                                    b_1 = buf_ms[54 + i_height * scan_line_len + i_width * 3];
                                    //中心点
                                    i_val = (int)b_1;
                                    g_val = i_val * 5;

                                    b_1 = buf_ms[54 + (i_height - 1) * scan_line_len + i_width * 3];
                                    //y-1
                                    i_val = (int)b_1;
                                    g_val = g_val - i_val;

                                    b_1 = buf_ms[54 + (i_height + 1) * scan_line_len + i_width * 3];
                                    i_val = (int)b_1;
                                    //y+1
                                    g_val = g_val - i_val;

                                    b_1 = buf_ms[54 + i_height * scan_line_len + (i_width + 1) * 3];
                                    i_val = (int)b_1;
                                    //x+1
                                    g_val = g_val - i_val;

                                    b_1 = buf_ms[54 + i_height * scan_line_len + (i_width - 1) * 3];
                                    i_val = (int)b_1;
                                    //x-1
                                    g_val = g_val - i_val;
                                     

                                    g_val = g_val - min_val;
                                    grey_val = (byte)((g_val*255)/most_val);

                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3] = grey_val;
                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3 + 1] = grey_val;
                                    buf_ms1[54 + i_height * scan_line_len + i_width * 3 + 2] = grey_val;
                                }//each pixel in one line 
                            }

                            break;
                        }
                }
            }
            SendMessage(GData.frm2_wnd_handle, GData.GRAY_FINISHED, 100, 100);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Thread workThread = new Thread(new ThreadStart(laplaceian_sharpen));
            workThread.IsBackground = true;
            workThread.Start();

            GData.frm2.Show(); 
        }
    }
}
