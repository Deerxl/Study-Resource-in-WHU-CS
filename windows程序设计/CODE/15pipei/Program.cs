using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm1 = new Form1();
            DataClass.frm1_wnd_handle = frm1.Handle;
            DataClass.ms_bmp_src = new MemoryStream(5000000);
            DataClass.ms_bmp_result = new MemoryStream(5000000);
            DataClass.ms_bmp_temp = new MemoryStream(1000000); 
            Application.Run(frm1);
        }
    }
}
