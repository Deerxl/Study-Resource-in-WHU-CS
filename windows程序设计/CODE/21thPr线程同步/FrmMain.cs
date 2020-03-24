using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace thPr
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        static extern int GetTickCount();

        static ManualResetEvent capture_terminate_e;
        static ManualResetEvent capture_this_one_e;
        public static ManualResetEvent[] me_cap = new ManualResetEvent[2];
        public const int WATCH_FILE = 0x500;
        static void Capture_screen()
        {
            int s_wid = Screen.PrimaryScreen.Bounds.Width;
            int s_height = Screen.PrimaryScreen.Bounds.Height;
            Bitmap b_1 = new Bitmap(s_wid, s_height);
            Graphics g_ = Graphics.FromImage(b_1);
            String init_dir_fn = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            String dest_fn = null;
            //用事件的方法来抓获图片 


            int index = WaitHandle.WaitAny(me_cap, 500);
            while (index != 0)
            {
                if (index == 1)
                {
                    dest_fn = init_dir_fn;
                    dest_fn += "\\bc\\";
                    dest_fn += GetTickCount().ToString();
                    dest_fn += "ab.bmp";
                    g_.CopyFromScreen(0, 0, 0, 0, new Size(s_wid, s_height));
                    b_1.Save(dest_fn, System.Drawing.Imaging.ImageFormat.Bmp);
                    capture_this_one_e.Reset();
                }
                index = WaitHandle.WaitAny(me_cap, 500);
            }
            g_.Dispose();
            b_1.Dispose();
        }



        public static string w_dir;
        public static ManualResetEvent e_wdirth_end; 
        private void button4_Click(object sender, EventArgs e)
        {
            e_wdirth_end.Reset();
            Thread workThread = new Thread(new ThreadStart(WatchDir));
            workThread.IsBackground = true;
            workThread.Start(); 
        }

        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        int lParam // second message parameter
        );
        public static IntPtr main_whandle;
        public static string strfileinfo;
        public static void WatchDir()
        { 
            long now_t = DateTime.Now.ToFileTime(); 
            DirectoryInfo d_info = new DirectoryInfo(w_dir);
            string new_filename;
            FileInfo[] f_ins = d_info.GetFiles();

            while (!e_wdirth_end.WaitOne(500))
            {
                for (int i = 0; i < f_ins.Length; i++)
                {
                    now_t = DateTime.Now.ToFileTime();
                    if (File.Exists(f_ins[i].FullName))
                    {
                        strfileinfo = string.Format("监视到文件{0}\r\n",f_ins[i].FullName);
                        SendMessage(main_whandle,WATCH_FILE,0,0);
                        new_filename = w_dir + "\\ref\\" + now_t.ToString() + f_ins[i].Name;
                        if (!File.Exists(new_filename))
                        {
                            File.Move(f_ins[i].FullName, new_filename);
                        }
                    }
                }
                f_ins = d_info.GetFiles();//重新获取新的目录信息
            }
        } 

        private void button5_Click(object sender, EventArgs e)
        {
            //终止文件目录监视线程
            e_wdirth_end.Set();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //初始抓屏终止事件为未结束 
            capture_terminate_e = new ManualResetEvent(false);
            //初始捕获终止状态为未结束 
            capture_this_one_e = new ManualResetEvent(false);
            //启动捕捉线程
            me_cap[0] = capture_terminate_e;
            me_cap[1] = capture_this_one_e;
            ThreadStart workStart = new ThreadStart(Capture_screen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();  
        }

        protected override void DefWndProc(ref Message m)
        {
            switch(m.Msg)
            {
                case WATCH_FILE:
                    textBox1.AppendText(strfileinfo);
                    textBox1.ScrollToCaret();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            capture_this_one_e.Set();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            capture_terminate_e.Set();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //启动文件目录监视线程
            e_wdirth_end = new ManualResetEvent(false);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                w_dir = folderBrowserDialog1.SelectedPath;
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            main_whandle=this.Handle;
        }

    }
}
