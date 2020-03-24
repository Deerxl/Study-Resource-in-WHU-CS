using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace wpq
{
    public partial class Form1 : Form
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        private const int VK_SNAPSHOT = 0x2C;
        //全局的事件  
        static int hKeyboardHook = 0; //键盘钩子句柄 
        //鼠标常量  
        public const int WH_KEYBOARD_LL = 13; //keyboard hook constant 
        HookProc KeyboardHookProcedure; //声明键盘钩子回调事件. 
        //声明键盘钩子的封送结构类型  
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode; //表示一个在1到254间的虚似键盘码  
            public int scanCode; //表示硬件扫描码  
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        //装置钩子的函数  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸下钩子的函数  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //下一个钩挂的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern int GetTickCount();

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam); 

        static ManualResetEvent capture_terminate_e;
        static ManualResetEvent capture_this_one_e;

        public Form1()
        {
            InitializeComponent();
        }

        private void Start_h()
        {
            //安装键盘钩子 
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                //hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                Process curProcess = Process.GetCurrentProcess();
                ProcessModule curModule = curProcess.MainModule;
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(curModule.ModuleName), 0);
                if (hKeyboardHook == 0)
                {
                    Stop_h();
                    throw new Exception("SetWindowsHookEx is failed.");
                }
            }


        }
        private void Stop_h()
        {
            bool retKeyboard = true;
            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            //如果卸下钩子失败 
            if (!(retKeyboard)) throw new Exception("UnhookWindowsHookEx failed.");
        }
        //键盘回调函数
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
			//lParam参数只是数据的内存地址
            KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
            if (wParam == WM_KEYDOWN || wParam == WM_KEYDOWN)
            {
                //PrintScreen负责执行单次捕捉
                if (keyData==Keys.PrintScreen)
                {
                    capture_this_one_e.Set();
                }
                //En负d责捕捉的任务结束
                
                if (keyData == Keys.End)
                {
                    capture_terminate_e.Set();
                }
                
            }  
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);  
        }

        static void  Capture_screen()
        { 
            int s_wid = Screen.PrimaryScreen.Bounds.Width;
            int s_height = Screen.PrimaryScreen.Bounds.Height; 

            Bitmap b_1 = new Bitmap(s_wid, s_height);
            Graphics g_ = Graphics.FromImage(b_1);

            String init_dir_fn = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            String dest_fn = null;


            //用事件的方法来抓获图片 
            //
            while(!capture_terminate_e.WaitOne(1,false))
            { 
                if (capture_this_one_e.WaitOne(-1, false))
                { 
                    dest_fn = init_dir_fn;
                    dest_fn += "\\bc\\";
                    dest_fn += GetTickCount().ToString();
                    dest_fn += "ab.bmp";
                    g_.CopyFromScreen(0, 0, 0, 0, new Size(s_wid, s_height));
                    b_1.Save(dest_fn, System.Drawing.Imaging.ImageFormat.Bmp);
                    capture_this_one_e.Reset();
                } 
            } 
            g_.Dispose();
            b_1.Dispose();  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Start_h();
            //初始捕获终止事件为未结束 
            capture_terminate_e=new ManualResetEvent(false);
            //初始捕获终止状态为未结束 
            capture_this_one_e = new ManualResetEvent(false); 
            //启动捕捉线程
            ThreadStart workStart = new ThreadStart(Capture_screen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stop_h();
            capture_terminate_e.Set();
        }
    }
}