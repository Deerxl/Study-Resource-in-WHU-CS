using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ChuJuan1
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
            DataFrm.fmMain = new FrmMain();
            DataFrm.fmAddTiXing = new FrmAddTiXing();
            DataFrm.fmEdKaoTi = new FrmEditKaoTi();
            DataFrm.fmNewTiKu = new FrmNewTiKu();

            DataFrm.msDataBuf = new System.IO.MemoryStream(8000000);
            KeChengTiKu.msBmp = new System.IO.MemoryStream(4000000);
            KeChengTiKu.bMsBuf = KeChengTiKu.msBmp.GetBuffer();

            Application.Run(DataFrm.fmMain);
        }
    }
}
