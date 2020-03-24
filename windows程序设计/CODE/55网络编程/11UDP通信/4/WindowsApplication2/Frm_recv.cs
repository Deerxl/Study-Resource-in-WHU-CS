using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace WindowsApplication2
{
    public partial class Frm_recv : Form
    { 
        public Frm_recv()
        {
            InitializeComponent();
        }
        public class MsgReceivedArgs : EventArgs
        {
            private readonly string recvMsg;

            public MsgReceivedArgs(string target)
            {
                recvMsg = target;
            }

            public string RecvMsg
            {
                get { return recvMsg; }
            }
        } 
        //定义UDP的接收端 
            public static UdpClient uReceiverSlot;

            //定义IPEndPoint端 
            public static IPEndPoint ipEp;

            //订阅者必须实现的委托， 用于处理收到的信息
            public delegate void MsgReceiverHandler(object sender, MsgReceivedArgs receiveMsg);

            //控制者对于委托的访问 
            public event MsgReceiverHandler OnMsgReceived;

            

            //UDPClient的异步接收 
            public void StartReceive()
            {
                uReceiverSlot.BeginReceive(new AsyncCallback(ReceiveCallback), ipEp);
                //Thread.Sleep(System.Threading.Timeout.Infinite);
            }

            //UDPClient异步接收到指令后的函数 
            public void ReceiveCallback(IAsyncResult iar)
            {
                IPEndPoint ipe = (IPEndPoint)ipEp;
                Byte[] receiveBytes = uReceiverSlot.EndReceive(iar, ref ipe);

                string receiveMessage = Encoding.ASCII.GetString(receiveBytes);

                //创建MsgReceivedArgs 对象 传给订阅者 
                if (receiveBytes.Length > 0)
                {
                    MsgReceivedArgs receiveInformation = new MsgReceivedArgs(receiveMessage);

                    if (OnMsgReceived != null)
                    {
                        OnMsgReceived(this, receiveInformation);
                    }
                }

                //重新进行接收 
                uReceiverSlot.BeginReceive(new AsyncCallback(ReceiveCallback), ipEp);
            }

        //public class UdpReceived
        //{
            

        //}//class UdpReceived 
        


        private void button1_Click(object sender, EventArgs e)
        {  
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //准备接收
            //UdpReceived udpR = new UdpReceived(8888);
            //Subscribe(udpR);
            //udpR.StartReceive(); 
 

            //生成一个对应端口的UDPClient，监听任意地址所发来的消息。
            if (ipEp==null)
            {
                ipEp = new IPEndPoint(IPAddress.Any, 8888);
                uReceiverSlot = new UdpClient(8888);
                this.OnMsgReceived += new MsgReceiverHandler(theUdp_OnMsgReceived);
                StartReceive();  
            }
            

        }
        delegate void SetTextCallback(string text); 

        public void W_gottext(String text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(W_gottext);
                this.Invoke(d, new object[] { text });

            }
            else {
                this.textBox1.Text = text;
            
            }
        }
        public void theUdp_OnMsgReceived(object sender, MsgReceivedArgs receiveMsg)
        {
            W_gottext(receiveMsg.RecvMsg);
        }  
    }
}