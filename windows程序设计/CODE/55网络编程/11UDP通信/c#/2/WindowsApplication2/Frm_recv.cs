using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace WindowsApplication2
{
    public partial class Frm_recv : Form
    {
        UdpClient receivingUdpClient = new UdpClient(8888);
        public Frm_recv()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        { 
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8888);

            try
            {

                for (int i1=0;i1<100 ;i1++ )
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref   RemoteIpEndPoint);
                    textBox1.Text = Encoding.ASCII.GetString(receiveBytes);
                }
                
            }
            catch (SocketException e2)
            {
                textBox1.Text = e2.Message;
            }   
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}