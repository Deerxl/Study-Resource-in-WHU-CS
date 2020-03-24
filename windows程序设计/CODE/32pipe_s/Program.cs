using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace pipe_s
{
    static class Program
    {
        private static Mutex mutex = null;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool firstInstance;
            mutex = new Mutex(true, @"Global\MutexSampleApp", out firstInstance);

            firstInstance = true;
            if (firstInstance)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("已经有本程序的实例运行，不再创建新实例");
            }

            
        }
    }
}
