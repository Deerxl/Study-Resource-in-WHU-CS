using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;

namespace udp_send
{
    public partial class Form_send : Form
    {
        public Form_send()
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

        public static Socket socket_send;
        public static IPEndPoint iep;
        public static EndPoint ep;

        public static byte[] send_data_buf;//设置缓冲数据流 
        public static int send_data_len;
        public static IPEndPoint RemoteIpEndPoint;
        private void button1_Click(object sender, EventArgs e)
        {
            //   
            try
            {
                byte[] b_txt;
                b_txt = Encoding.Default.GetBytes(DateTime.Now.ToString() + " " + textBox2.Text);
                //b_txt = Encoding.Default.GetBytes(textBox2.Text);
                send_data_len=b_txt.Length;
                Array.Copy(b_txt, send_data_buf, b_txt.Length);
                socket_send.SendTo(send_data_buf, send_data_len, SocketFlags.None, RemoteIpEndPoint); 
            }
            catch (Exception e2)
            {
                textBox1.Text = e2.Message;
            }   
        }

        private void Form_send_Load(object sender, EventArgs e)
        {
            send_data_buf = new byte[1024];
            socket_send = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议 
            socket_send.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);//设置该scoket实例的发送形式
            //发送端
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 9095);//初始化一个发送广播和指定端口的网络端口实例
             
        } 

    }
}
