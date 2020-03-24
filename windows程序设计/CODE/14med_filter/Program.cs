using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace med_filter
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
            GData.frm2 = new Form2();
            GData.frm2.Hide();
            GData.frm2_wnd_handle = GData.frm2.Handle;
            GData.ms_bmp = new MemoryStream(5000000);
            GData.ms_bmp1 = new MemoryStream(5000000);
            Application.Run(frm1);
        }
    }
}
