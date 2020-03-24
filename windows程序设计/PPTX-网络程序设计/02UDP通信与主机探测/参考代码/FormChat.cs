using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace UdpChatExample
{
    public partial class FormChat : Form
    { 
        private UdpClient receiveUdpClient; 
        private UdpClient sendUdpClient; 
        private const int port = 18001; 
        IPAddress ip; 
        IPAddress remoteIp;
        
        //定义消息常数
        public const int RECV_DATA = 0x500;
        public const int SEND_DATA = 0x501;
        public static IntPtr main_wnd_handle;//主窗体句柄
        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        int lParam // second message parameter
        );

        private static string sendMessage;
        private static IPEndPoint iep;
        private static string receiveMessage;
        private static IPEndPoint remote;

        public FormChat()
        {
            InitializeComponent();
            //获取本机可用IP地址
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            ip = ips[ips.Length - 1];
            //为了在同一台机器调试，此IP也作为默认远程IP
            remoteIp = ip;
            textBoxRemoteIP.Text = remoteIp.ToString();
            textBoxSend.Text = "你好！";
        }
        private void FormChat_Load(object sender, EventArgs e)
        {
            //创建一个线程接收远程主机发来的信息
            Thread myThread = new Thread(ReceiveData);
            //将线程设为后台运行
            myThread.IsBackground = true;
            myThread.Start();
            textBoxSend.Focus(); 
            main_wnd_handle = this.Handle;
        }


        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case RECV_DATA:
                    listBoxReceive.Items.Add(string.Format("来自{0}：{1}", remote, receiveMessage));
                    listBoxReceive.SelectedIndex = listBoxReceive.Items.Count - 1;
                    listBoxReceive.ClearSelected(); 
                    break;
                case SEND_DATA:
                    listBoxStatus.Items.Add(string.Format("向{0}发送：{1}", iep, sendMessage));
                    textBoxSend.Text = "";
                    textBoxSend.Focus();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        
        private void ReceiveData()
        {
            IPEndPoint local = new IPEndPoint(ip, port);
            receiveUdpClient = new UdpClient(local);
            remote = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    //关闭udpClient时此句会产生异常
                    byte[] receiveBytes = receiveUdpClient.Receive(ref remote);
                    receiveMessage = Encoding.Unicode.GetString(
                        receiveBytes, 0, receiveBytes.Length);
                    SendMessage(main_wnd_handle, RECV_DATA, 100, 100);
                }
                catch
                {
                    break;
                }
            }
        }
        private void buttonSend_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(SendMessage);
            t.IsBackground = true;
            sendMessage = textBoxSend.Text;
            t.Start();
        }
        /// <summary>发送数据到远程主机</summary>
        
        private void SendMessage()
        { 
            sendUdpClient = new UdpClient(0);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(sendMessage);
            iep = new IPEndPoint(remoteIp, port);
            try
            {
                sendUdpClient.Send(bytes, bytes.Length, iep);
                SendMessage(main_wnd_handle, SEND_DATA, 100, 100);
            }
            catch (Exception ex)
            { 
            }
        } 
    }
}
