using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace uwol
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket_send;
            IPEndPoint iep;
            EndPoint ep;
            //设置缓冲数据流
            byte[] send_data_buf; 
            int send_data_len;
            IPEndPoint RemoteIpEndPoint;
            send_data_buf = new byte[1024];
            //初始化一个Scoket协议 
            socket_send = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //设置该scoket实例的发送形式
            socket_send.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            //发送端
            //初始化一个发送广播和指定端口的网络端口实例
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 9095);
            try
            {

                //获取的网卡的MAC地址为44-37-E6-02-57-72
                byte[] b_txt1 = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                byte[] b_txt2 = { 0x44, 0x37, 0xe6, 0x03, 0x16, 0x01 };
                string mac_file = "mac.txt";
                if(File.Exists(mac_file))
                {
                    StreamReader sr = new StreamReader(mac_file);
                    sr.ReadLine();
                    sr.ReadLine();
                    sr.ReadLine();
                    string astr = "";
                    astr=sr.ReadLine();
                    astr = astr.Substring(0,17);
                    astr.Replace("-","");
                    for (int i = 0; i < 6; )
                    {
                        int ch_i = 0; 
                        ch_i=(int)astr[i];

                        //b_txt2[i]=;
                    }
                    sr.Close();

                }
                
                Buffer.BlockCopy(b_txt1, 0, send_data_buf, 0, 6);
                for (int i = 1; i <= 16; i++)
                {
                    Buffer.BlockCopy(b_txt2, 0, send_data_buf, 6 * i, 6);
                }
                send_data_len = 6 * 17;
                socket_send.SendTo(send_data_buf, send_data_len, SocketFlags.None, RemoteIpEndPoint);
            }
            catch (Exception e2)
            {
                
            }   
        }
    }
}
