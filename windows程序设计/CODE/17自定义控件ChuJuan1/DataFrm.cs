using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
/*创建日期 :2014.05.11
 *修改日期 :2014.05.11
 *类名称    :DataFrm
 * 类说明  ：管理所有窗体对象的静态类
 */
namespace ChuJuan1
{
    public static class DataFrm
    {
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
        IntPtr hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        int lParam // second message parameter
        );


        public static FrmMain fmMain;
        public static FrmAddTiXing fmAddTiXing;
        public static FrmEditKaoTi fmEdKaoTi;
        public static FrmNewTiKu fmNewTiKu;
        public static MemoryStream msDataBuf;
    }
}
