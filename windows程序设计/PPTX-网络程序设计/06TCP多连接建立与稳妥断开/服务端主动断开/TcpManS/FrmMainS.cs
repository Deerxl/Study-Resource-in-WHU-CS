using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace TcpManS
{
    public partial class FrmMainS : Form
    {
        public FrmMainS()
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
        //总Socket连接数
        public static int indexOfConnectedSockets;
        public static IntPtr mainWndhandle;
        //定义消息常数  
        public const int UPDATE_INFO = 0x502;
        public const int BEGIN_LISTEN = 0x503;
        public const int END_LISTEN = 0x504;
        public const int WINDOW_RESTORE = 0x505;
        //信息字符串数组
        public static string[] Infos;
        public static Socket listenSocket;
        public static IPEndPoint localEndPoint;
        //终止监听事件控制变量
        public static ManualResetEvent mrEventTermListen;
        public static StateObject[] ClientStateData;
        //标记当前总的回复数
        public static int TotalReplyNum;
        public static byte[] ReplyBuf;

        private void FrmMainS_Load(object sender, EventArgs e)
        {
            mainWndhandle = this.Handle;
            Infos = new string[11];

            mrEventNoteThrd = new ManualResetEvent[2];
            for (int i = 0; i < 2; i++)
            {
                mrEventNoteThrd[i] = new ManualResetEvent(false);
            }
            mrEventTermListen = new ManualResetEvent(false);
            localEndPoint = new IPEndPoint(IPAddress.Parse("172.16.201.85"), 8133);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //创建对象数组仅为10个空指针
            ClientStateData = new StateObject[10];
            //实例化StateObject对象
            for (int i = 0; i < 10; i++)
            {
                ClientStateData[i] = new StateObject();
                //分配数据缓冲内存空间
                ClientStateData[i].buffer = new byte[1024];
                //初始时数据长度设为0
                ClientStateData[i].datalen = 0;
            }
            indexOfConnectedSockets = -1;
            TotalReplyNum = 0;
            //创建回复信息的缓冲区
            ReplyBuf = new byte[1024];
        }




        //listen线程体--
        static void threadListen()
        {
            LingerOption _lingerOption = new LingerOption(true, 3);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, _lingerOption);
            listenSocket.Blocking = false;//设定其为异步  
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(10);
            listenSocket.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listenSocket);
            Infos[0] = "正在监听";
            SendMessage(mainWndhandle, UPDATE_INFO, 100, 200);
            //保持监听状态
            mrEventTermListen.WaitOne();
            Infos[0] = "监听结束";
            SendMessage(mainWndhandle, UPDATE_INFO, 100, 200);
            listenSocket.Close();
        }


        //负责接收连接的回调函数 
        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket dataHandle = listenSocket.EndAccept(ar);
            Interlocked.Increment(ref indexOfConnectedSockets);
            StateObject so = ClientStateData[indexOfConnectedSockets];
            //新客户连接,使用一个StateObject对象,每次设置一个编号
            so.clientNum = indexOfConnectedSockets;
            so.workSocket = dataHandle;
            Infos[indexOfConnectedSockets] = string.Format("第{0}个客户连接到服务端，本地Sock信息为{1}",
                indexOfConnectedSockets, dataHandle.LocalEndPoint.ToString());
            SendMessage(mainWndhandle, UPDATE_INFO, 100, 200);
            so.workSocket.BeginReceive(so.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReceiveCallback), so);

            //持续接收后续的客户连接
            listenSocket.BeginAccept(
                   new AsyncCallback(AcceptCallback),
                   listenSocket);
        }

        public class StateObject
        {
            public int clientNum;
            public int datalen;
            //与客户端通信的Socket对象
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            //与客户端通信的缓冲区
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }


        //实现多次接收的回调函数
        public static void ReceiveCallback(IAsyncResult ar)
        {
            //数据到来
            // Retrieve the state object and the handler socket
            // from the asynchronous state object. 
            StateObject state = (StateObject)ar.AsyncState; 
            Socket handler = state.workSocket;
            SocketError sckErr;
            // Read data from the client socket. Accquire the SocketError
            try
            {
                int bytesRead = handler.EndReceive(ar, out sckErr);
                if (bytesRead > 0)
                {
                    //获取数据总长度
                    state.datalen = BitConverter.ToInt32(state.buffer, 0);
                    //Clear the buffer
                    state.sb.Remove(0, state.sb.Length);
                    Infos[state.clientNum] = handler.RemoteEndPoint.ToString() + ":" +
                        Encoding.UTF8.GetString(state.buffer, 4, state.datalen);
                    SendMessage(mainWndhandle, UPDATE_INFO, 100, 200);
                    //    byte[] byteData = Encoding.UTF8.GetBytes(string.Format("这是来自服务器的总第{0}个响应",TotalReplyNum));
                    //    Array.Clear(ReplyBuf, 0, 1024); 
                    //    Array.Copy(byteData, ReplyBuf, byteData.Length);

                    //    //发送回复信息
                    //    handler.BeginSend(ReplyBuf, 0, ReplyBuf.Length, 0,
                    //new AsyncCallback(SendCallback), handler);   
                    //继续接收后续的网络数据
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state); 
                }
                else
                {//数据长度为0，FD_CLOSE消息，.net平台无法接收。
                    if(handler.Connected)
                    {
                        Infos[state.clientNum] = string.Format("{0}已发送ShutDown请求，已关闭与{0}的TCP连接", 
                            handler.RemoteEndPoint.ToString());
                        state.workSocket.Shutdown(SocketShutdown.Send);
                        //Close方法将释放资源，但句柄标识仍在，指针值不为空，Connected属性为false
                        state.workSocket.Close();
                        SendMessage(mainWndhandle, UPDATE_INFO, 100, 200); 
                    }
                }  
            }
            catch (Exception sockError)
            {
                MessageBox.Show(sockError.Message);
            } 
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent); 
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close(); 
            }
            catch (Exception e)
            {
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            //启动监听的线程 
            ThreadStart workStart = new ThreadStart(threadListen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();
        }

        //字符串信息重新赋值
        public void ReFreshInfo()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 11; i++)
            {
                sb.AppendLine(Infos[i]);
            }
            textBox1.Text = sb.ToString();
        }

        /// 重写窗体的消息处理函数 
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case UPDATE_INFO:
                    ReFreshInfo();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        //线程更新程序
        //mrEventNoteThrd[1].Set();
        public static ManualResetEvent[] mrEventNoteThrd;
        //0 终止线程事件
        //1 线程更新状态信息
        //负责Socket对象数组的信息更新显示等
        //public static void SockMannager()
        //{ 
        //    int iWaitRet;
        //    iWaitRet = WaitHandle.WaitAny(mrEventNoteThrd); 
        //    while (iWaitRet!=0)//判断线程是否终结
        //    {
        //        if(iWaitRet==1)
        //        {
        //            //线程更新状态信息
        //            SendMessage(mainWndhandle, UPDATE_INFO, 100, 200);
        //            mrEventNoteThrd[1].Reset(); 
        //        }
        //        iWaitRet=WaitHandle.WaitAny(mrEventNoteThrd);
        //    } 
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            //结束监听
            mrEventTermListen.Set();
        }

    }
}
