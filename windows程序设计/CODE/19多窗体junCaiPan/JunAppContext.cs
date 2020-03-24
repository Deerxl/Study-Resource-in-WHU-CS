using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace junCaiPan
{
    public class JunAppContext:ApplicationContext
    {
        public FrmMain fmMain;
        public JunAppContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            //创建主窗体
            fmMain = new FrmMain();
            fmMain.Closed += new EventHandler(this.OnFormClosed);
            fmMain.Closing += new CancelEventHandler(this.OnFormClosing);
            //启动工作线程
            ThreadStart theStart = new ThreadStart(CaiPanDClass.DrawBoard);
            Thread theThread = new Thread(theStart);
            theThread.IsBackground = true;
            theThread.Start();
            //显示主窗体
            fmMain.Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {

        }
        private void OnFormClosing(object sender, CancelEventArgs  e)
        { 
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            if (sender is FrmMain)
            {
                //结束工作线程
                CaiPanDClass.meTermiNateDraw.Set();
                ExitThread();
            } 
        }
    }
}
