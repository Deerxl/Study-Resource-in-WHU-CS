using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace med_filter
{
    class GData
    {
        public static MemoryStream ms_bmp, ms_bmp1;
        public static Bitmap bp_1;   
        public static IntPtr frm2_wnd_handle;//form1窗体句柄
        public static Form2 frm2;
        public const int GRAY_FINISHED = 0x501;
    }
}
