using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace smtp
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );

        public static IntPtr main_wnd_handle;

        //定义消息常数 
        public const int UPDATE_INFO = 0x500;
        public static ManualResetEvent MRE_check_end;
private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
{
    String token = (string)e.UserState;
    if (e.Cancelled)
    {
        send_str = "邮件发送取消\r\n";
        SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
    } 
    if (e.Error != null)
    {
        send_str = string.Format("[{0}] {1}", token, e.Error.ToString());
        SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
    }
    else
    {
        send_str = "邮件已发送\r\n";
        SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
    } 
    MRE_check_end.Set();
}
        public static string send_str;
        /// 重写窗体的消息处理函数 
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case UPDATE_INFO:
                    textBox1.AppendText(send_str); ;
                    textBox1.ScrollToCaret();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        static void thr_smtp_con()
        {
            SmtpClient client = new SmtpClient("smtp.126.com");
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential("zscleonet", "goodstudent");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailAddress from = new MailAddress("zscleonet@126.com",
               "李老师" + (char)0xD8 + "中山学院",
            System.Text.Encoding.UTF8);
            MailAddress to = new MailAddress("zscleo@126.com");
            MailMessage message = new MailMessage(from, to);
            message.Body = "This is a test e-mail message sent by an application. ";
            // Include some non-ASCII characters in body and subject.
            string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
            message.Body += Environment.NewLine + someArrows;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = "好好学习" + someArrows;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);
            string userState = "test message1";
            send_str = "开始发送邮件\r\n";
            SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            client.SendAsync(message, userState);
            MRE_check_end.WaitOne();
            send_str = "邮件发送完成\r\n";
            SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            message.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MRE_check_end.Reset();
            ThreadStart thStart = new ThreadStart(thr_smtp_con);
            Thread thr = new Thread(thStart);
            thr.Start();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            main_wnd_handle = this.Handle;
            MRE_check_end = new ManualResetEvent(false);
        }




    }
}
