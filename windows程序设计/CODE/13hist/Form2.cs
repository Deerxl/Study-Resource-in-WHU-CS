using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            line_pen = new Pen(Color.Black, (float)0.5);

            green_pen = new Pen(Color.Green, (float)1.5);
            red_pen = new Pen(Color.Red, (float)1.5);
            blue_pen = new Pen(Color.Blue, (float)1.5);
            gray_pen = new Pen(Color.Black, (float)1.5);
             
            dash_pen = new Pen(Color.Black, (float)0.5);
            dash_pen.DashStyle = DashStyle.Dash;
            start_point = new Point();
            end_point = new Point();
        }
        public static Pen line_pen,green_pen,red_pen,blue_pen,gray_pen;
        //public static Brush dash_brush;
        public static Pen dash_pen;
        public static Point start_point,end_point,last_point,cur_point;
        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            start_point.X=0;
            start_point.Y=0;
            end_point.X=10;
            end_point.Y=20;
            int x_delta = 10;
            int y_delta = 10;
            //e.Graphics.DrawLine(line_pen,start_point,end_point);
            e.Graphics.DrawLine(line_pen, x_delta, y_delta-5, x_delta, 500 + y_delta);
            e.Graphics.DrawLine(line_pen, x_delta, y_delta+500, 520+x_delta, 500 + y_delta);

            e.Graphics.DrawLine(dash_pen, x_delta,  y_delta, 500 + x_delta,  y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta, 100+y_delta, 500+x_delta, 100 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta, 200 + y_delta, 500 + x_delta, 200 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta, 300 + y_delta, 500 + x_delta, 300 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta, 400 + y_delta, 500 + x_delta, 400 + y_delta);

            e.Graphics.DrawLine(dash_pen, x_delta+100, y_delta, 100 + x_delta, 500+y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta + 200, y_delta, 200 + x_delta, 500 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta + 300, y_delta, 300 + x_delta, 500 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta + 400, y_delta, 400 + x_delta, 500 + y_delta);
            e.Graphics.DrawLine(dash_pen, x_delta + 500, y_delta, 500 + x_delta, 500 + y_delta);
            
            if(radioButton1.Checked)
            {
                //下面是曲线绘制
                if (checkBox1.Checked)//选中红
                {//绘制红色直方图
                    last_point.X = x_delta;
                    last_point.Y = y_delta + 500;
                    for (int i = 0; i < 256; i++)
                    {
                        cur_point.X = x_delta + i << 1;
                        cur_point.Y = y_delta + 500 - (Int32)((DataClass.buf_red_hist[i]) / 90.0);
                        e.Graphics.DrawLine(red_pen, last_point, cur_point);
                        last_point = cur_point;
                    }

                }

                if (checkBox2.Checked)//选中绿
                {//绘制绿色直方图
                    last_point.X = x_delta;
                    last_point.Y = y_delta + 500;
                    for (int i = 0; i < 256; i++)
                    {
                        cur_point.X = x_delta + i << 1;
                        cur_point.Y = y_delta + 500 - (Int32)((DataClass.buf_green_hist[i]) / 90.0);
                        e.Graphics.DrawLine(green_pen, last_point, cur_point);
                        last_point = cur_point;
                    }
                }
                if (checkBox3.Checked)//选中蓝
                {//绘制蓝色直方图
                    last_point.X = x_delta;
                    last_point.Y = y_delta + 500;
                    for (int i = 0; i < 256; i++)
                    {
                        cur_point.X = x_delta + i << 1;
                        cur_point.Y = y_delta + 500 - (Int32)((DataClass.buf_blue_hist[i]) / 90.0);
                        e.Graphics.DrawLine(blue_pen, last_point, cur_point);
                        last_point = cur_point;
                    }
                }
                if (checkBox4.Checked)//选中灰
                {//绘制灰色直方图
                    last_point.X = x_delta;
                    last_point.Y = y_delta + 500;
                    for (int i = 0; i < 256; i++)
                    {
                        cur_point.X = x_delta + i << 1;
                        cur_point.Y = y_delta + 500 - (Int32)((DataClass.buf_gray_hist[i]) / 50.0);
                        e.Graphics.DrawLine(gray_pen, last_point, cur_point);
                        last_point = cur_point;
                    }
                }
            }
            if (radioButton2.Checked)
            {//以下是柱状图绘制
                if (checkBox4.Checked)//选中灰
                {//绘制灰色直方图
                    for (int i = 0; i < 256; i++)
                    {
                        cur_point.X = x_delta + i << 1;
                        cur_point.Y = y_delta + 500 - (Int32)((DataClass.buf_gray_hist[i]) / 50.0);
                        e.Graphics.DrawLine(gray_pen, last_point, cur_point);
                        
                    }
                }
            }
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
