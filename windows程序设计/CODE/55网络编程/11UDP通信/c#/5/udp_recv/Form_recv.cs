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

namespace udp_recv
{
    public partial class Form_recv : Form
    {
        public Form_recv()
        {
            InitializeComponent();
        }


        public static IntPtr main_wnd_handle;

        //定义消息常数 
        public const int TRAN_UDP_IN = 0x500;
        public const int TRAN_SET_PROGRESS = 0x501;


        //动态链接库引入
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );

        //定义UDP的接收端 
        public static Socket socket_recv;
        public static EndPoint remote_ep;  


        public static byte[] recv_data_buf;
        public static int recv_data_len;

        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case TRAN_UDP_IN:
                    {
                        string recv_str = Encoding.Default.GetString(recv_data_buf, 0, recv_data_len);
                        switch(recv_str)
                        {
                            case "go":
                                {   
                                    axWindowsMediaPlayer1.Ctlcontrols.play();
                                    break;
                                }
                            case "pause":
                                {
                                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                                    break;
                                }
                                
                        }
                        textBox1.Text += Encoding.Default.GetString(recv_data_buf, 0, recv_data_len)+"\r\n";
                        break;
                    }
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }



        public void theUdp_OnMsgReceived(object sender, EventArgs e)
        { 
            
        }
        public class Obj_one
        { 
        }

        private void button1_Click(object sender, EventArgs e)
        {  
            //生成一个对应端口的UDPClient，监听任意地址所发来的消息。
            //开始接收 
            Obj_one ob_1=new Obj_one();
            socket_recv.BeginReceiveFrom(recv_data_buf, 0, 1024, SocketFlags.None, ref remote_ep, ReceiveCallback, ob_1);           
        }  

        //UDPClient异步接收到指令后的函数 
        public void ReceiveCallback(IAsyncResult ar)
        {
             
            try
            {
                recv_data_len = socket_recv.EndReceiveFrom(ar, ref remote_ep);  
                SendMessage(main_wnd_handle, TRAN_UDP_IN, 100, 100);
                if (recv_data_len > 0)
                {
                    Obj_one ob_1 = new Obj_one();
                    socket_recv.BeginReceiveFrom(recv_data_buf, 0, 1024, SocketFlags.None, ref remote_ep, ReceiveCallback, ob_1);
                }
                else
                {
                    socket_recv.Close();
                }

            }
            // Store the exception message.
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);    
            }         
        }
         
        private void Form_recv_Load(object sender, EventArgs e)
        { 
            recv_data_buf = new byte[1024];
            main_wnd_handle = this.Handle; 
            socket_recv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议  
            //接收端
            remote_ep = new IPEndPoint(IPAddress.Any, 9095);//初始化一个侦听局域网内部所有IP和指定端口 
            
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 9095);//初始化一个发送广播和指定端口的网络端口实例
            socket_recv.Bind(iep);//绑定这个实例


            //设置音乐文件路径
            axWindowsMediaPlayer1.URL = @"D:\09-10下\网络程序设计\11UDP通信\Canon.mp3";
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

    }
}
