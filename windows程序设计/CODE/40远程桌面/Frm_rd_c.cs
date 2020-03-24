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

namespace remote_desk_c
{
    public partial class Frm_rd_c : Form
    {
        public Frm_rd_c()
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
        public static MemoryStream ms_cap_pic;
        public static ManualResetEvent terminate_capture;
        public static int cap_period;
        public static Socket client_sock;
        

        private void button1_Click(object sender, EventArgs e)
        {
            //开始拷屏线程启动 
            ThreadStart workStart = new ThreadStart(thread_capture_send);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();

            //设置发送间隔
            cap_period = Int32.Parse(textBox1.Text);
        }
        //线程体--定时获取屏幕 
        static void thread_capture_send()
        {
            //连接服务器
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.1.100"), Int32.Parse("8133"));
            // Create a TCP/IP socket.
            Socket client_sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            client_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

            try
            {
                client_sock.Blocking = true;
                client_sock.Connect(remoteEP);
                try
                {
                    //定时屏幕保存， 

                    int s_wid = Screen.PrimaryScreen.Bounds.Width;
                    int s_height = Screen.PrimaryScreen.Bounds.Height;
                    Bitmap b_1 = new Bitmap(s_wid, s_height);
                    Graphics g_ = Graphics.FromImage(b_1);
                    long each_screen_data_len;

                    byte[] SendDataBuffer = new byte[1024];//数据发送缓存
                    byte[] ReadDataBuffer = new byte[1024];//数据接收缓存
                    byte[] b_data_len;

                    int tran_count = 0;
                    int ms_data_read_count;
                    int numberOfByteRead;
                    int replay_code;
                    //拷屏部分应用控制来循环处理

                    do
                    {
                        g_.CopyFromScreen(0, 0, 0, 0, new Size(s_wid, s_height));
                        //为了响应迅速,使用MemoryStream内存为数据存区
                        //设置流为最开始的位置
                        ms_cap_pic.Seek(0, SeekOrigin.Begin);
                        b_1.Save(ms_cap_pic, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //数据量大小得到--通过流的位置来决定要发送数据的大小                
                        each_screen_data_len = ms_cap_pic.Position;

                        //所有的数据全部转化成字节后，拼装在一起发送出去。 
                        //1将数组值清空
                        Array.Clear(SendDataBuffer, 0, 1024);
                        //将数据命令数转化为字节数组
                        b_data_len = BitConverter.GetBytes((int)10);//10代表开始发送一屏
                        Array.Copy(b_data_len, 0, SendDataBuffer, 0, 4);
                        client_sock.Send(SendDataBuffer, 1024, SocketFlags.None);

                        //等待服务器响应代码
                        Array.Clear(ReadDataBuffer, 0, 1024);
                        numberOfByteRead = client_sock.Receive(ReadDataBuffer, 1024, SocketFlags.None);
                        replay_code = BitConverter.ToInt32(ReadDataBuffer, 0);


                        tran_count = 0;
                        ms_cap_pic.Seek(0, SeekOrigin.Begin);
                        do
                        {
                            b_data_len = BitConverter.GetBytes((int)20);//20代表屏幕图像数据
                            Array.Copy(b_data_len, 0, SendDataBuffer, 0, 4);
                            //read from memory to the buffer
                            ms_data_read_count = ms_cap_pic.Read(SendDataBuffer, 4, 1020);
                            //write to network
                            client_sock.Send(SendDataBuffer, ms_data_read_count + 4, SocketFlags.None);
                            //等待服务器响应代码
                            Array.Clear(ReadDataBuffer, 0, 1024);
                            numberOfByteRead = client_sock.Receive(ReadDataBuffer, 1024, SocketFlags.None);
                            replay_code = BitConverter.ToInt32(ReadDataBuffer, 0);

                            tran_count += ms_data_read_count;
                        } while (tran_count < each_screen_data_len);

                        b_data_len = BitConverter.GetBytes((int)30);//30代表图像数据发送完毕
                        Array.Copy(b_data_len, 0, SendDataBuffer, 0, 4);
                        client_sock.Send(SendDataBuffer, ms_data_read_count, SocketFlags.None);
                        //等待服务器响应代码
                        Array.Clear(ReadDataBuffer, 0, 1024);
                        numberOfByteRead = client_sock.Receive(ReadDataBuffer, 1024, SocketFlags.None);
                        replay_code = BitConverter.ToInt32(ReadDataBuffer, 0);
                        //间隔一定时间
                        Thread.Sleep(cap_period);
                    } while (!terminate_capture.WaitOne(1));

                    //1将数组值清空
                    Array.Clear(SendDataBuffer, 0, 1024);
                    //发送命令数据50
                    b_data_len = BitConverter.GetBytes((int)50);//50代表客户端退出
                    Array.Copy(b_data_len, 0, SendDataBuffer, 0, 4);
                    client_sock.Send(SendDataBuffer, ms_data_read_count, SocketFlags.None);
                    //等待服务器响应代码
                    Array.Clear(ReadDataBuffer, 0, 1024);
                    numberOfByteRead = client_sock.Receive(ReadDataBuffer, 1024, SocketFlags.None);
                    replay_code = BitConverter.ToInt32(ReadDataBuffer, 0);
                    client_sock.Shutdown(SocketShutdown.Both);
                    //拷屏部分应用控制来循环处理  

                }
                catch (SocketException se3)
                {
                    MessageBox.Show("客户端异常" + se3.Message);

                }
            }
            catch (SocketException se1)
            {
                MessageBox.Show("SocketException 客户端连接不到服务器呢" + se1.Message);
            }
            catch (Exception se2)
            {
                MessageBox.Show("客户端异常" + se2.Message);
            }
            //线程代码--end
        }

        private void Frm_rd_c_Load(object sender, EventArgs e)
        {
            ms_cap_pic = new MemoryStream(5000000);
            cap_period = Int32.Parse(textBox1.Text); 
            terminate_capture = new ManualResetEvent(false); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            terminate_capture.Set();
        }


    }
}
