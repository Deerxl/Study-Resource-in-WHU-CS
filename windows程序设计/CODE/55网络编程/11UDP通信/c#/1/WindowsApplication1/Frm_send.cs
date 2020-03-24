using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace WindowsApplication1
{
    public partial class Frm_send : Form
    {
        public Frm_send()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 8888);
            //   
            try
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(textBox1.Text);
                udpClient.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                //textBox1.Text =  
            }
            catch (Exception e2)
            {
                textBox1.Text = e2.Message;
            }   
        }
    }
}