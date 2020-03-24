using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace filetr
{
    public partial class Frm_C : Form
    {
        //文件发送客户端，负责与服务器连接，传送文件
        //主窗体句柄
        public static IntPtr main_wnd_handle;
        public static IntPtr main_label2_handle;
        public static String tran_file_name;
        public Frm_C()
        {
            InitializeComponent();
        }

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window 
        int Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );
        //定义消息常数 
        public const int TRAN_INFO = 0x500;
        public const int TRAN_SET_PROGRESS = 0x501;
        public const int TRAN_UPDATE_PROGRESS = 0x502;
        public const int TRAN_FINISHED = 0x503;
        public static String Tr_info;
        static void thread_client_trans()
        {//线程入口
            //线程流程
            
            //1.设置进度条值
            //2.连接远程服务器
            //3.循环发送数据，每次更新进度值
            //4.发送完毕，设置完成消息

            //1.设置进度条值
            Tr_info = "开始连接服务器";
            PostMessage(main_wnd_handle, TRAN_INFO, 100, 200);
            FileInfo tr_finf = new FileInfo(tran_file_name);
            //PostMessage(main_wnd_handle, TRAN_SET_PROGRESS, 100, (int)tr_finf.Length);
            Tr_info = "正在传送文件";
            SendMessage(main_wnd_handle, TRAN_SET_PROGRESS, 100, 5000); 
            //2.连接远程服务器
            //IPHostEntry ipHostInfo = Dns.Resolve("127.0.0.1");
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Int32.Parse("8131"));

            // Create a TCP/IP socket.
            Socket client_sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client_sock.Blocking = true;
                client_sock.Connect(remoteEP);
                try{
                    // Create the NetworkStream for communicating with the remote host.
                    NetworkStream client_NetStream = new NetworkStream(client_sock, FileAccess.Write, true);
                    
                    FileInfo tran_file_in = new FileInfo(tran_file_name);
                    //数据暂存缓存，开始数据打包
                    byte[] SendDataBuffer = new byte[1024];
                    //1将数组值清空
                    Array.Clear(SendDataBuffer, 0, 1024);
                    //2long整数返回字节长度为 8 的字节数组
                    byte[] b_file_len = BitConverter.GetBytes(tran_file_in.Length); 
                    Array.Copy(b_file_len, 0, SendDataBuffer, 0, 8);
                    //3文件字节数---4字节
                    byte[] b_filename_len = BitConverter.GetBytes(tran_file_in.Name.Length);
                    Array.Copy(b_filename_len, 0, SendDataBuffer, 8, 4);
                    //4文件名长度---8字节
                    byte[] b_file_name = Encoding.ASCII.GetBytes(tran_file_in.Name);
                    //文件名字节数量因文件名会有所不同
                    Array.Copy(b_file_name, 0, SendDataBuffer, 8 + 4, b_file_name.Length);

                    //虽然客户端可以确定每次发送多少个字节，但接收端无法确定，因此约定先发送1024字节，有浪费
                    client_NetStream.Write(SendDataBuffer, 0, 1024);
                    //使流发送出去
                    client_NetStream.Flush();
                    //通知窗体发送文件的长度
                    SendMessage(main_wnd_handle, TRAN_SET_PROGRESS, 100, (int)tran_file_in.Length);
                    int tran_count = 0;
                    int file_read_count = 0;
                    //FileStream可以读取任意类型文件，StreamReader只能读文本文件 
                    FileStream fs_file =tran_file_in.OpenRead();

                    do
                    {
                        file_read_count = fs_file.Read(SendDataBuffer, 0, 1024);
                        client_NetStream.Write(SendDataBuffer, 0, file_read_count);
                        client_NetStream.Flush();
                        tran_count += file_read_count;
                        SendMessage(main_wnd_handle, TRAN_UPDATE_PROGRESS, 100, tran_count);
                    } while (client_NetStream.CanWrite && fs_file.Position<fs_file.Length);


                    SendMessage(main_wnd_handle, TRAN_FINISHED, 100, 100);
                    fs_file.Close();

                }catch(SocketException se3)
                {
                    MessageBox.Show("客户端异常"+se3.Message);               
                
                }



            }
            catch (SocketException se1)
            {
                MessageBox.Show("SocketException"+se1.Message);
            }
            catch (Exception se2)
            {
                MessageBox.Show("客户端异常" + se2.Message);
            }
            
            



            Thread.Sleep(1000);
            SendMessage(main_wnd_handle, TRAN_FINISHED, 100, 200);


        }
        /// 重写窗体的消息处理函数 
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {  
                case TRAN_INFO:
                    label2.Text = Tr_info;
                    break;
                case TRAN_SET_PROGRESS:
                    progressBar1.Maximum = (int)m.LParam;
                    progressBar1.Value = 0;
                    label2.Text = "正在传送文件...";
                    break;
                case TRAN_UPDATE_PROGRESS: 
                    progressBar1.Value = (int)m.LParam ;
                    break;
                case TRAN_FINISHED:
                    label2.Text = "文件已经传输完成";
                    progressBar1.Value = progressBar1.Maximum;
                    break; 
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void Frm_C_Load(object sender, EventArgs e)
        {
            main_wnd_handle = this.Handle;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //用户发出命令进行连接
            //发送条件，有文件信息
            //1.检查具有文件信息，能否进行完整发送
            //2.如果没有文件信息，提示用户进行文件选择
            //文件信息齐全则启动线程

            //1.检查具有文件信息，能否进行完整发送
            if(!File.Exists(tran_file_name))
            {
                MessageBox.Show("你没有选择要传输的文件，不能传送");
                return;
            }

            label2.Text = "启动连接线程进行服务器连接......";
            //2.开始文件传输线程
            ThreadStart workStart = new ThreadStart(thread_client_trans);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //利用对话框选择要传输的文件
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tran_file_name = openFileDialog1.FileName;
                FileInfo finf = new FileInfo(tran_file_name);
                label1.Text = finf.Name;
            }
        }
    }
}