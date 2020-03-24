using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Net;

namespace TcpManC
{
    public partial class FrmMainC : Form
    {
        public FrmMainC()
        {
            InitializeComponent();
        }

        public static string[] Infos;
        public static IntPtr mainWndHandle;
        public static IPEndPoint remoteEP;
        public static StateObject[] ClientState; 
        public const int UPDATE_INFO = 0x502;
        public const int UPDATE_SOCKINFO = 0x503;
        public static int socketid;
        public static int selectedIndex;
        public static Socket[] ClientSock;

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
            IntPtr hWnd,
            int Msg,
            int wParam,
            int lParam);


        // State object of Client Socket for receiving data from remote device.
        public class StateObject
        {
            //Socket对象编号
            public int clientNum;
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Send buffer.
            public byte[] buffer = new byte[BufferSize];
            //Data length
            public int Datalen;
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
        
        private void FrmMainC_Load(object sender, EventArgs e)
        {
            mainWndHandle = this.Handle;
            Infos=new string[11];

            ClientSock=new Socket[10];
            ClientState=new StateObject[10];
            for (int i = 0; i < 10;i++ )
            {
                ClientSock[i]=new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,ProtocolType.Tcp);
                ClientState[i]=new StateObject();
                ClientState[i].buffer=new byte[1024];
                ClientState[i].clientNum = i;
                ClientState[i].workSocket = ClientSock[i];
                Infos[i] = string.Format("第{0}个Socket对象创建成功，未连接", i);
            }
            selectedIndex = -1; 
            remoteEP = new IPEndPoint(IPAddress.Parse("172.16.201.85"), Int32.Parse("8133"));             
            ReFreshInfo();
        } 
                
        //刷新显示字符串信息
        public void ReFreshInfo()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 11;i++ )
            {
                sb.AppendLine(Infos[i]);
            }
            textBox2.Text = sb.ToString();
        }
        

        //重写窗体的消息处理函数
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            { 
                //显示信息更新
                case UPDATE_INFO:
                    ReFreshInfo();
                    break;
                //Socket信息更新
                case UPDATE_SOCKINFO: 
                    listBox1.Items[socketid]=ClientSock[socketid].LocalEndPoint.ToString();
                    ReFreshInfo();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break; 
            } 
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex=listBox1.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(selectedIndex!=-1)
            {
                //检测是否已经连接
                if (!ClientSock[selectedIndex].Connected)
                {
                ClientSock[selectedIndex].BeginConnect(remoteEP, 
                    new AsyncCallback(ConnectCallback), ClientState[selectedIndex]);
                } 
            } 
        }

        
        //连接的回调函数
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            { 
                // Retrieve the socket from the state object.
                StateObject cltso = (StateObject)ar.AsyncState; 
                // Complete the connection.
                cltso.workSocket.EndConnect(ar);
                Infos[cltso.clientNum]=string.Format("Socket connected to {0}",
                    cltso.clientNum.ToString()); 
                socketid=cltso.clientNum;
                SendMessage(mainWndHandle, UPDATE_SOCKINFO, 100, 100);
                cltso.workSocket.BeginReceive(cltso.buffer,0,StateObject.BufferSize,0,
                    new AsyncCallback(ReceiveCallback), cltso); 
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            } 
        }
        //实现多次数据接收的回调函数
        public static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler=state.workSocket;
            SocketError sckErr;
            try
            {
                int bytesRead = handler.EndReceive(ar, out sckErr);
                if (bytesRead > 0)
                {
                    //接收到应用数据
                }
                else {//bytesRead==0 
                    Infos[state.clientNum]=string.Format("服务端已经断开连接{0}",state.clientNum);
                    state.workSocket.Close();
                    SendMessage(mainWndHandle,UPDATE_INFO,100,100);
                }
            }catch(Exception sockError)
            {
                MessageBox.Show(sockError.Message);
            }
        
        }
        private void button11_Click(object sender, EventArgs e)
        {
            //发送数据
            if (selectedIndex != -1)
            {
                //检测是否已经连接
                if (ClientSock[selectedIndex].Connected)
                {
                    // Convert the string data to byte data using UTF8 encoding.
                    byte[] byteData = Encoding.UTF8.GetBytes(textBox1.Text);
                    byte[] bdataLen = BitConverter.GetBytes(byteData.Length);
                    ClientState[selectedIndex].Datalen=byteData.Length;
                    Array.Copy(bdataLen, ClientState[selectedIndex].buffer, 4);
                    Array.Copy(byteData, 0, ClientState[selectedIndex].buffer, 4, byteData.Length);
                    //Clear bytes of StringBuilder
                    ClientState[selectedIndex].sb.Remove(0, ClientState[selectedIndex].sb.Length);
                    ClientState[selectedIndex].sb.Append(textBox1.Text);  
                    // Begin sending the data to the remote device.
                    ClientState[selectedIndex].workSocket.BeginSend(ClientState[selectedIndex].buffer, 0, byteData.Length + 4, 0,
                        new AsyncCallback(SendDataCallback), ClientState[selectedIndex]); 
                }
            } 
        }
        //发送数据的回调函数
        private static void SendDataCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the StateObject from the state object.
                StateObject cltso = (StateObject)ar.AsyncState;  
                // Complete sending the data to the remote device.
                int bytesSent = cltso.workSocket.EndSend(ar);
                Infos[cltso.clientNum] = string.Format("发送：{0}", cltso.sb.ToString());
                SendMessage(mainWndHandle, UPDATE_INFO, 100, 100);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //发送数据
            if (selectedIndex != -1)
            {
                //检测是否已经连接
                if (ClientSock[selectedIndex].Connected)
                {
                    ClientSock[selectedIndex].Shutdown(SocketShutdown.Send); 
                }
            }
        } 
    }
}
