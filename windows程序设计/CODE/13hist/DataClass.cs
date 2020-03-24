using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace WindowsFormsApplication1
{
    public static class DataClass
    {
        public static  MemoryStream ms_bmp;
        public static Bitmap bp_1;
        public static Bitmap bp_grey;
        public static Int32[] buf_red_hist;//红分量直方图
        public static Int32[] buf_green_hist;//绿分量直方图
        public static Int32[] buf_blue_hist;//蓝分量直方图
        public static Int32[] buf_gray_hist;//灰分量直方图
    }
}
