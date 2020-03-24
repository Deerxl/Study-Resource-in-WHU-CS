using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace junCaiPan
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
            //获取本机IP地址
            CaiPanDClass.FetchLocalIPAddr();
            CaiPanDClass.iClientCount = 0; 
            //初始化图片资源
            CaiPanDClass.InitPicRes();
            //初始化棋盘，没有放子
            CaiPanDClass.InitBoardQiZi();  
            CaiPanDClass.junContext = new JunAppContext();
            //传递主窗体句柄值给线程
            CaiPanDClass.mainWndPtr = CaiPanDClass.junContext.fmMain.Handle; 
            //启动裁判端UDP通信线程
            ThreadStart theCaiPanUdpThreadStart = new ThreadStart(UdpCaipanTran.CaiPanUdpTranThread);
            Thread theCaiPanUdpThread = new Thread(theCaiPanUdpThreadStart);
            theCaiPanUdpThread.IsBackground = true;
            theCaiPanUdpThread.Start(); 
            Application.Run(CaiPanDClass.junContext); 
        }
    }
}
