using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadStart workStart = new ThreadStart(thr_smtp_con);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();
        }


        static void thr_smtp_con()
        {
            SmtpClient client = new SmtpClient("smtp.126.com");
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential("zscleonet", "goodstudent");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailAddress from = new MailAddress("zscleonet@126.com",
            " 李老师" + (char)0xD8 + " 中山学院",
            System.Text.Encoding.UTF8);
            MailAddress to = new MailAddress("zscleo@126.com");
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Body = "整天看手机,一点好处都没,迟早眼要瞎";
            // Include some non-ASCII characters in body and subject.
            string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
            mailMessage.Body += Environment.NewLine + someArrows;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.Subject = "13软件工程B" + someArrows;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
            client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);
            string userState = "test message1";
            send_str = " 开始发送邮件 \r\n";
            // current working directory.
            string attachfile = "Kid1.png"; 
            Attachment atcData = new Attachment(attachfile, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = atcData.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(attachfile);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(attachfile);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(attachfile);
            // Add the file attachment to this e-mail message.
            mailMessage.Attachments.Add(atcData);
            SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            client.SendAsync(mailMessage, userState);
            MRE_check_end.WaitOne();
            send_str = " 邮件发送完成 \r\n";
            SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            mailMessage.Dispose();
        }
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case UPDATE_INFO:
                    label1.Text += send_str;
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window
        int Msg, // message
        int wParam, // rst message parameter
                    int lParam // second message parameter
        );
        public static string send_str;

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            String token = (string)e.UserState;
            if (e.Cancelled)
            {
                send_str = " 邮件发送取消 \r\n";
                SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            }
            if (e.Error != null)
            {
                send_str = string.Format("[{0}] {1}", token, e.Error.ToString());
                SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            }
            else
            {

                send_str = " 邮件已发送 \r\n";
                SendMessage(main_wnd_handle, UPDATE_INFO, 100, 100);
            }
            MRE_check_end.Set();
        }


        //定义消息常数
        public const int UPDATE_INFO = 0x500;

        public static ManualResetEvent MRE_check_end;
        public static IntPtr main_wnd_handle;

        private void Form1_Load(object sender, EventArgs e)
        {
            MRE_check_end = new ManualResetEvent(false);
            main_wnd_handle = this.Handle;
        }
    }
}
