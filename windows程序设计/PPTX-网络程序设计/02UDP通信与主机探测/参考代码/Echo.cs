using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace echo
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        public class Obj_one
        {
        }
        public static Obj_one ob_1;

        public static IPEndPoint ipeRemoteClient;
        public static int iUdprecvDataLen;
        public static string strInfo;
        public static byte[] udpRecvDataBuf;
        public static byte[] udpSendDataBuf;
        public static Socket sockUdpRecv;
        public static Socket sockUdpSend;

        public void ReceiveUdpCallback(IAsyncResult ar)
        {
            try
            {
                EndPoint tempRemoteEP = (EndPoint)ipeRemoteClient;
                iUdprecvDataLen = sockUdpRecv.EndReceiveFrom(ar, ref tempRemoteEP);
                strInfo = Encoding.UTF8.GetString(udpRecvDataBuf, 0, iUdprecvDataLen);
                udpSendDataBuf = new byte[1024];
                ipeRemoteClient = (IPEndPoint)tempRemoteEP;
                IPEndPoint iep = new IPEndPoint(ipeRemoteClient.Address, 9096);
                byte[] databyte = Encoding.UTF8.GetBytes("hello from server");
                sockUdpSend.SendTo(databyte, iep);
                //继续接收UDP数据包
                sockUdpRecv.BeginReceiveFrom(udpRecvDataBuf, 0, 1024, SocketFlags.None, ref tempRemoteEP, ReceiveUdpCallback, ob_1);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        } 


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "正在响应";
            button1.Enabled = false;
            sockUdpSend = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议,用于发送数据 
            //开始进行UDP数据包接收，接收到广播包就进行回复，表示服务器存在
            sockUdpRecv = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议,用于接收数据 
            //初始化一个发送广播和指定端口的网络端口实例
            ipeRemoteClient = new IPEndPoint(IPAddress.Any, 9095);
            //绑定接收端口的网络端口实例
            EndPoint iep = new IPEndPoint(IPAddress.Any, 9095);
            sockUdpRecv.Bind(iep);//绑定这个实例
            ob_1 = new Obj_one();
            sockUdpRecv.BeginReceiveFrom(udpRecvDataBuf, 0, 1024, 
                SocketFlags.None, ref iep, ReceiveUdpCallback, ob_1);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            udpRecvDataBuf = new byte[1024];
        } 
    }
}
