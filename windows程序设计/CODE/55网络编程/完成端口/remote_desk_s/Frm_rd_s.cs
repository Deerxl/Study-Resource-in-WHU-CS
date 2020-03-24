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
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace remote_desk_s
{
    public partial class Frm_rd_s : Form
    {
        public Frm_rd_s()
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
        public const int UPDATE_SCREEN = 0x502;
        public const int BEGIN_LISTEN = 0x503;
        public const int END_LISTEN = 0x504;
        public const int WINDOW_RESTORE = 0x505;

        public static Socket S_Listen_sock;
        public static Socket S_client_sock;

        public static ManualResetEvent User_Terminate_listen;
        public static ManualResetEvent event_send_data;  
        public static bool windows_is_full;
        public static bool form_loaded = false;

        public static String send_txt;

        private void button1_Click(object sender, EventArgs e)
        {
            //开始listen--启动listen线程 
            ThreadStart workStart = new ThreadStart(thread_listen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();
             
            form_loaded = true;
            windows_is_full = true;
        }
         
        public class Accep_Object
        {

        }
        //listen线程体--
        static void thread_listen()
        {
            IPAddress[] host_ip = Dns.GetHostAddresses(Dns.GetHostName());
            S_Listen_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LingerOption _lingerOption = new LingerOption(true, 3);
            S_Listen_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, _lingerOption);
            S_Listen_sock.Blocking = false;//设定其为异步 
            IPEndPoint host_end = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Int32.Parse("8133"));
            //IPEndPoint host_end = new IPEndPoint(IPAddress.Parse("172.16.13.188"), Int32.Parse("8133"));
            
            User_Terminate_listen.Reset();
            S_Listen_sock.Bind(host_end);//开始绑定
            S_Listen_sock.Listen(3);//开始监听

            Accep_Object Ac_state = new Accep_Object();
            S_Listen_sock.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    Ac_state);
            SendMessage(main_wnd_handle, BEGIN_LISTEN, 100, 200);

            User_Terminate_listen.WaitOne();
            //关闭所有的子socket，结束监听 
            //不应该马上调用关闭，因为这会清空S_Listen_sock对象
            //将会迟些时候关闭 
            SendMessage(main_wnd_handle, END_LISTEN, 100, 200);
        }

        //负责接收连接的回调函数 
        public static void AcceptCallback(IAsyncResult ar)
        {
            //有新的客户端连接  
            if (S_Listen_sock == null)
            {   //原因比较复杂，暂不作解释 
                S_client_sock = S_Listen_sock.EndAccept(ar);
                MessageBox.Show("新客户已经开始连接服务器");
            }
            else
            {
                //MessageBox.Show("新客户已经连接到服务器or服务端停止了监听"); 
                S_client_sock = S_Listen_sock.EndAccept(ar);
                S_client_sock.Blocking = true;
                //每次新的Client到来则启动一个新的线程，利用新的Socket与客户交互
                ThreadStart clientWorkStart = new ThreadStart(thread_recv_display);
                Thread clientThread = new Thread(clientWorkStart);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        //接收网络数据的线程体--显示到屏幕上
        static void thread_recv_display()
        {
            //线程流程
            //1.利用client_socket创建NetWorkStream
            //2.接收图像信息
            try
            {
                S_client_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

                byte[] ReceiveDataBuffer = new byte[1024];
                byte[] SendDataBuffer = new byte[1024];
                byte[] b_data_reply;
                int numberOfBytesRead;
                long data_cmd;
                bool bl_not_end_recv = true;
                do
                {
                    event_send_data.WaitOne();
                    byte[] bs = Encoding.Default.GetBytes(send_txt);
                    Array.Copy(bs,SendDataBuffer,bs.Length);
                    S_client_sock.Send(SendDataBuffer, bs.Length, SocketFlags.None);
                    event_send_data.Reset();
                      
                } while (bl_not_end_recv);
            }
            catch (SocketException Se1)
            {
                MessageBox.Show("SocketException:" + Se1.Message);
            }
            catch (Exception Se2)
            {
                MessageBox.Show(Se2.Message);
            }

        }//thread_recv_display 

        /// 重写窗体的消息处理函数 
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            { 
                case BEGIN_LISTEN:
                    label1.Text = "开始服务";
                    break;
                case END_LISTEN:
                    label1.Text = "终止服务";
                    break;  
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
 

        private void Frm_rd_s_Load(object sender, EventArgs e)
        { 
            User_Terminate_listen = new ManualResetEvent(false);
            event_send_data = new ManualResetEvent(false);
            main_wnd_handle = this.Handle;//窗体句柄初始化 
            send_txt = textBox1.Text;
            button1_Click(this, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //停止listen线程
            User_Terminate_listen.Set();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            send_txt = textBox1.Text;
            event_send_data.Set();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        } 

    }
}
