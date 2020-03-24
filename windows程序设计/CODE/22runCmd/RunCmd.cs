using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace runCmd
{
    public partial class RunCmd : Form
    {
        public RunCmd()
        {
            InitializeComponent();
        }

        public static Process cmdP;
        public static StreamWriter cmdStreamInput;
        private static StringBuilder cmdOutput = null;


        //private const int WM_SETREDRAW = 0x100;
        public const int TRAN_FINISHED = 0x500;
        public const int WM_VSCROLL = 0x0115;
        public const int SB_BOTTOM = 0x0007;
        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );
        public static IntPtr main_whandle;
        public static IntPtr text_whandle;
        private void button1_Click(object sender, EventArgs e)
        {
            cmdStreamInput.WriteLine(textBox1.Text); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cmdP = new Process();
            cmdP.StartInfo.FileName = "cmd.exe";
            cmdP.StartInfo.CreateNoWindow = true;
            cmdP.StartInfo.UseShellExecute = false;
            cmdP.StartInfo.RedirectStandardOutput = true;
            cmdP.StartInfo.RedirectStandardInput = true;
            cmdOutput = new StringBuilder("");

            cmdP.OutputDataReceived += new DataReceivedEventHandler(strOutputHandler);
            cmdP.Start();

            cmdStreamInput = cmdP.StandardInput;
            cmdP.BeginOutputReadLine();  
            
        }
        
        private static void strOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            cmdOutput.AppendLine(outLine.Data);
            SendMessage(main_whandle, TRAN_FINISHED, 0, 0);
            SendMessage(text_whandle, WM_VSCROLL, SB_BOTTOM, 50);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cmdP.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = cmdOutput.ToString();
        }
        
        private void RunCmd_Load(object sender, EventArgs e)
        {
            main_whandle = this.Handle;
            text_whandle = textBox2.Handle;
        }



        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case TRAN_FINISHED:
                    textBox2.Text = cmdOutput.ToString(); 
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
    }
}
