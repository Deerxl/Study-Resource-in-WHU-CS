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
        public static MemoryStream ms_bmp_src, ms_bmp_result, ms_bmp_temp;
        public static Bitmap bp_1;//原始大图
        public static Bitmap bp_2;//模板
        public static bool bm_ready;
        public static IntPtr frm1_wnd_handle;
        public const int GRAY_FINISHED = 0x501;
    }
}
