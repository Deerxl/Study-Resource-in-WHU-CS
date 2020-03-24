using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;

namespace filetrs
{
    public partial class Frm_s : Form
    {
        //文件发送服务端
        //主窗体句柄
        public static IntPtr main_wnd_handle;
        public Frm_s()
        {
            InitializeComponent();
        }
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );
        //定义消息常数 
        public const int BEGIN_LISTEN = 0x500;
        public const int END_LISTEN = 0x501;

        public const int TRAN_CLIENT_ACCEPT = 0x502;
        public const int TRAN_CLIENT_TRAN = 0x503;

        //用于设置传输进度
        public const int TRAN_FILE_NAMES = 0x504;
        public const int TRAN_SET_PROGRESS = 0x505;
        public const int TRAN_UPDATE_PROGRESS = 0x506;
        public const int TRAN_FINISHED = 0x507;

        private void Frm_s_Load(object sender, EventArgs e)
        {
            main_wnd_handle = this.Handle;
            User_Terminate_listen = new ManualResetEvent(false);
            socket_list = new ArrayList();
        }

        public static ManualResetEvent User_Terminate_listen;
        public static ArrayList socket_list;
        public static Socket S_Listen_sock;
        public static Socket S_client_sock;
        public static String tran_file_name;
        

        public class Accep_Object
        {

        }
        
        static void thread_listen()
        { //监听线程
            //1.开始监听
            //2.等待用户关闭命令，但是设置为各子线程必须完成状态才结束 

            //监听线程入口
            //线程流程
            //1.获取主机信息
            //2.启动listen
            //3.使用begin_accept完成异步
            //4.检查全局变量，等待停止信号到来
            //5.检查所有已经连接的客启端，向每个客户端发送close命令
            //6.等待客启端关闭...比较困难
            //6.如果所有连接客启端已经关闭，则发出close命令

            IPAddress[] host_ip = Dns.GetHostAddresses(Dns.GetHostName()); 
            
            S_Listen_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LingerOption _lingerOption = new LingerOption(true, 3);
            S_Listen_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, _lingerOption);  
            S_Listen_sock.Blocking = false;//设定其为异步
            //IPAddress.Parse("127.0.0.1");
            //IPEndPoint host_end = new IPEndPoint(host_ip[0], 8128);
            IPEndPoint host_end = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Int32.Parse("8131"));

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
            S_Listen_sock.Close();
            SendMessage(main_wnd_handle, END_LISTEN, 100, 200); 
        }

        //负责接收连接的回调函数 
        public static void AcceptCallback(IAsyncResult ar)
        {
            //有新的客户端连接 
            // Get the socket that handles the client request.

            if (S_Listen_sock == null)
            {   //在此增加此语句有两个原因
                //1.---Callback是在消息队列里面循环调用的，当发生了主监听不存在的时候，
                //callback仍然会被调用。所以没有真正的我们理解上的客户到来，但是此函数被正实的调用了
                //2.在dotnet平台中资源的回收虽然是由自动回收机制完成的，但是对于socket来说，是调用的
                //底层的socket接口，而这些方法本身具有资源回收的功能，所以在socket方法调用Close方法
                //的时候，socket已经变成空值，也就是NULL，但是Callback却实际上会被调用，
                //因此发生ObjectDisposedException 异常
                //MessageBox.Show("listen Socket is null，监听已经停止");
                //int closesocket(SOCKET s)的作用是关闭指定的socket，并且回收其所有的资源。
                //int shutdown(SOCKET s,  int how)则是禁止在指定的socket s上禁止进行由how指定的操作，但并不对资源进行回收，shutdown之后而closesocket之前s还不能再次connect或者WSAConnect.
                //通过上面的说明，socket.Close方法实际上就是调用了closesocket ，资源当然就不存在了
                //
                S_client_sock = S_Listen_sock.EndAccept(ar);
                MessageBox.Show("新客户已经开始连接服务器");
            }else
            {
                //MessageBox.Show("新客户已经连接到服务器");
                S_client_sock = S_Listen_sock.EndAccept(ar);
                S_client_sock.Blocking = true;
                //每次新的Client到来则启动一个新的线程，利用新的Socket与客户交互
                ThreadStart clientWorkStart = new ThreadStart(thr_client_recv);
                Thread clientThread = new Thread(clientWorkStart);
                clientThread.IsBackground = true;
                clientThread.Start();
            } 

        }

        static void thr_client_recv()
        {//单个接收线程入口
            //线程流程
            //1.利用client_socket创建NetWorkStream
            //2.接收文件信息，设置进度条
            //3.利用networkstream接收所有文件内容，并更新进度条
            //4.发送完毕，设置完成消息  
            try
            {
                // Create the NetworkStream for communicating with the remote host.
                NetworkStream client_NetStream=new NetworkStream(S_client_sock,FileAccess.Read,true);
                byte[] ReceiveDataBuffer = new byte[1024];
                //Array.Clear(ReceiveDataBuffer, 0, 1024);
                //1.获取文件头信息
                client_NetStream.Read(ReceiveDataBuffer, 0, 1024);
                //解析基本文件信息 
                //2.得到文件长度值
                long file_len = BitConverter.ToInt64(ReceiveDataBuffer, 0);
                SendMessage(main_wnd_handle, TRAN_SET_PROGRESS, 100, (int)file_len);
                //3.得到文件名长度值
                int file_name_len = BitConverter.ToInt32(ReceiveDataBuffer, 8);
                tran_file_name = Encoding.ASCII.GetString(ReceiveDataBuffer, 8 + 4, file_name_len);
                SendMessage(main_wnd_handle, TRAN_FILE_NAMES, 100, 200);

                string new_file_name = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + tran_file_name;
                if (File.Exists(new_file_name)) File.Delete(new_file_name);//重复传相同名称文件时，删掉原来接收到的文件

                FileInfo fi_file = new FileInfo(new_file_name);
                FileStream fs_newfile = fi_file.OpenWrite();
                int tran_count = 0;
                int numberOfBytesRead = 0;
                do
                {
                    numberOfBytesRead = client_NetStream.Read(ReceiveDataBuffer, 0, 1024);
                    fs_newfile.Write(ReceiveDataBuffer, 0, numberOfBytesRead);
                    fs_newfile.Flush();
                    tran_count += numberOfBytesRead;
                    SendMessage(main_wnd_handle, TRAN_UPDATE_PROGRESS, 100, tran_count);  
                }while (client_NetStream.DataAvailable && tran_count < file_len);
                SendMessage(main_wnd_handle, TRAN_FINISHED, 100, 100);
                fs_newfile.Close(); 

            }
            catch (SocketException Se1)
            {
                MessageBox.Show("SocketException:"+Se1.Message);
            }
            catch (Exception Se2)
            {
                MessageBox.Show("服务器端"+Se2.Message);
            } 

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //启动监听线程
            ThreadStart workStart = new ThreadStart(thread_listen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();
        }

        /// 重写窗体的消息处理函数 
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                //接收自定义消息 ，并显示其参数 
                case BEGIN_LISTEN:
                     //m.WParam, m.LParam; 
                    label4.Text = "正在监听"; 
                    break;
                case END_LISTEN:
                    //m.WParam, m.LParam;  
                    label4.Text = "结束监听";
                    break;
                case TRAN_CLIENT_ACCEPT:
                    //m.WParam, m.LParam; 
                    label4.Text = "新客户到达";
                    break;
                case TRAN_CLIENT_TRAN:
                    //m.WParam, m.LParam; 
                    label4.Text = "正在传输中";
                    break;
                case TRAN_FILE_NAMES:
                    //设置文件名
                    //m.WParam, m.LParam; 
                    label1.Text = tran_file_name;
                    break; 
                case TRAN_SET_PROGRESS:
                    //客端传来文件大小信息，开始传输
                    progressBar1.Maximum = (int)m.LParam;
                    progressBar1.Value = 0; 
                    break;
                case TRAN_UPDATE_PROGRESS:
                    //更新文件传输状态
                    progressBar1.Value = (int)m.LParam;
                    break;
                case TRAN_FINISHED:
                    //文件传输完成
                    progressBar1.Value = progressBar1.Maximum;
                    label4.Text = "文件传输完成";
                    break; 
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {   //用户按下按钮停止监听
            User_Terminate_listen.Set();
        }

    }
}