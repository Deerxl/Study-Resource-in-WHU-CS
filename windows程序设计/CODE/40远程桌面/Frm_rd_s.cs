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
        FormBorderStyle original_style;
        FormWindowState original_state;

        public static MemoryStream ms_cap_pic;
        public static Bitmap bp_screen;
        public static Bitmap bp_full;
        public static Bitmap bp_small;

        public static bool windows_is_full;
        public static bool form_loaded = false;


        private void button1_Click(object sender, EventArgs e)
        {
            //开始listen--启动listen线程 
            ThreadStart workStart = new ThreadStart(thread_listen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();

            original_style = this.FormBorderStyle;
            original_state = this.WindowState;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            bp_screen = bp_full;
            form_loaded = true;
            windows_is_full = true;
        }

        private void Frm_rd_s_MouseClick(object sender, MouseEventArgs e)
        {
            //右键单击
            if (e.Button == MouseButtons.Right)
            {
                //进入普通界面
                this.FormBorderStyle = original_style;
                this.WindowState = original_state;
                foreach (Control cc in this.Controls)
                {
                    cc.Visible = true;
                }
                //恢复时显示普通图
                bp_screen = bp_small;
                windows_is_full = false;
            }
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
            IPEndPoint host_end = new IPEndPoint(IPAddress.Parse("192.168.1.100"), Int32.Parse("8133"));

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
                    //1.获取网络数据
                    numberOfBytesRead = S_client_sock.Receive(ReceiveDataBuffer, 1024, SocketFlags.None);
                    //2.获取格式命令头信息
                    data_cmd = BitConverter.ToInt32(ReceiveDataBuffer, 0);
                    b_data_reply = BitConverter.GetBytes((int)data_cmd + 1);//data_cmd+1代表对data_cmd的响应
                    Array.Clear(SendDataBuffer, 0, 1024);
                    Array.Copy(b_data_reply, SendDataBuffer, 4);
                    S_client_sock.Send(SendDataBuffer, 1024, SocketFlags.None);
                    switch (data_cmd)
                    {
                        case 10:    //开始发送一屏
                            ms_cap_pic.Seek(0, SeekOrigin.Begin);
                            break;
                        case 20:    //屏幕图像数据  
                            //write to memory stream
                            ms_cap_pic.Write(ReceiveDataBuffer, 4, numberOfBytesRead - 4);
                            break;
                        case 30:    //屏幕数据发送完毕 
                            //发送消息，更新屏幕图像 
                            bp_full = (Bitmap)Bitmap.FromStream(ms_cap_pic);
                            SendMessage(main_wnd_handle, UPDATE_SCREEN, 100, 100);
                            break;
                        case 40:    //
                            break;
                        case 50:    //50客户端退出 
                            SendMessage(main_wnd_handle, WINDOW_RESTORE, 100, 100);
                            bl_not_end_recv = false;
                            break;
                    }
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
                case UPDATE_SCREEN:
                    bp_screen = bp_full;
                    //全屏显示一个桌面 
                    foreach (Control cc in this.Controls)
                    {
                        cc.Visible = false;
                    }
                    this.Invalidate();
                    break;
                case BEGIN_LISTEN:
                    label1.Text = "开始服务";
                    break;
                case END_LISTEN:
                    label1.Text = "终止服务";
                    break;
                case WINDOW_RESTORE:
                    label1.Text = "用户中断发送屏幕";
                    //进入普通界面 
                    foreach (Control cc in this.Controls)
                    {
                        cc.Visible = true;
                    }
                    //恢复时显示普通图
                    bp_screen = bp_small;
                    windows_is_full = false;
                    this.FormBorderStyle = original_style;
                    this.WindowState = original_state;
                    break;

                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void Frm_rd_s_Paint(object sender, PaintEventArgs e)
        {
            if (form_loaded)
            {
                e.Graphics.DrawImage(bp_screen, 0, 0);
            }
        }

        private void Frm_rd_s_Load(object sender, EventArgs e)
        {
            ms_cap_pic = new MemoryStream(5000000);
            bp_full = new Bitmap("fsdg.bmp");
            bp_small = new Bitmap("essdge.bmp");
            User_Terminate_listen = new ManualResetEvent(false);
            main_wnd_handle = this.Handle;//窗体句柄初始化 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //停止listen线程
            User_Terminate_listen.Set();
        }




    }
}
