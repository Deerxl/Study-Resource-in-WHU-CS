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
using PacketDotNet;
using SharpPcap;

namespace sendarp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
[DllImport("Iphlpapi.dll")]
private static extern int SendARP(
    Int32 dest,Int32 host,ref Int64 mac,ref Int32 length);
[DllImport("Ws2_32.dll")]
private static extern Int32 inet_addr(string ip); 
private string RemoteIpToMac(string destIp)
{
    string temp1="",temp2="";
    try
    {
        StringBuilder macAddress = new StringBuilder();
        Int32 remote = inet_addr(destIp); 
        Int64 macInfo = new Int64();
        Int32 length = 6; 
        SendARP(remote, 0, ref macInfo, ref length);

        if (length == 0)
        {
            temp2 = "";
        }
        else
        {
            //两个字符代表一个字节
            temp1 = Convert.ToString(macInfo, 16).PadLeft(12, '0').ToUpper();
            for (int i = 0; i < 6; i++)
            {
                temp2 = temp2 + temp1.Substring(10 - i * 2, 2);
            } 
        } 
        
    }catch(Exception err)
    {
        MessageBox.Show("IpToMac" + err.Message);
    }
    return temp2;
}

private void sendWol(string destMac)
{
    try
    {  
        byte[] macAddr = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            macAddr[i] = (byte)int.Parse(destMac.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }
        Socket socket_send;
        //创建一个进行UDP广播的Socket对象
        socket_send = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 
        socket_send.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1); 
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 9095);
        byte[] send_data_buf = new byte[1024]; 
        byte[] b_txt1 = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }; 
        Buffer.BlockCopy(b_txt1, 0, send_data_buf, 0, 6);
        for (int i = 1; i <= 16; i++)
        {
            Buffer.BlockCopy(macAddr, 0, send_data_buf, 6 * i, 6);
        }
        int send_data_len = 6 * 17;
        socket_send.SendTo(send_data_buf, send_data_len, SocketFlags.None, RemoteIpEndPoint);
        socket_send.Close();                
    }
    catch (Exception err)
    {
        MessageBox.Show(err.Message);
    }

}


        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Trim().Length==0)
            {
                return;
            }
            string temstr = RemoteIpToMac(textBox1.Text.Trim());
            textBox2.Text = temstr;
            if(temstr.Length!=0)
            {
                sendWol(temstr);
            }
            //PacketDotNet.EthernetPacketType.
            //
        }
    }
}
