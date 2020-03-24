using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileRecv
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        } 
        public const int FILETRAN_UPPROG = 0x502;
        public const int FILETRAN_UPTEXT = 0x503;
        public const int FILETRAN_SETPROG = 0x504;
        public static string strSaveFileName;
        public static ManualResetEvent meEndThr;//结束线程
        public static ManualResetEvent[] meAry;
        public static ManualResetEvent mePkgCome;//数据包到达 
        //UDP数据发送端口与接收端口值-文件接收端，与发送端相反
        public const int UDPRECV_PORT = 9096;
        public const int UDPSEND_PORT = 9095;
        public static Socket skUdpSend;
        public static Socket skUdpRecv;
        public static EndPoint remoteEp;
        public static IPEndPoint remoteIPEp;
        public static int iUdpRecvPkgLen;//接收到的数据包长度
        public static IntPtr wndHandle;
        [DllImport("User32.dll")]
        private static extern int SendMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        int lParam
        );
        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            { 
                case FILETRAN_UPPROG://更新进度条值
                    progressBar1.Value = (int)m.LParam;
                    break;
                case FILETRAN_SETPROG://设置进度条取值区间
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = (int)m.LParam;
                    break;
                case FILETRAN_UPTEXT://更新文本信息 
                    label1.Text = strMsg;
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        public void InitData()
        {
            meAry = new ManualResetEvent[2];
            meEndThr = new ManualResetEvent(false);
            mePkgCome = new ManualResetEvent(false);
            meAry[0] = meEndThr;
            meAry[1] = mePkgCome;
            //绑定接收UDP数据回调函数
            udpRecvDataBuf = new byte[1024];
            remoteIPEp = new IPEndPoint(IPAddress.Any, 0); //创建IPEndPoint对象
            skUdpRecv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, UDPRECV_PORT);
            skUdpRecv.Bind(iep); 
            remoteEp=(EndPoint)remoteIPEp;
            skUdpRecv.BeginReceiveFrom(udpRecvDataBuf, 0, 1024,
                SocketFlags.None, ref remoteEp, UdpReceiveCallBack, new object());
            //创建发送数据Socket对象与数据缓冲区
            udpSendDataBuf = new byte[1024];
            skUdpSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ThreadStart thrStart = new ThreadStart(FileRecvThr);
            Thread thrT = new Thread(thrStart);
            thrT.IsBackground = true;
            thrT.Start();
            bThrIsIdle = true;
        }
        public static string strMsg;//在窗体上显示的线程信息
        public static int iPkgType;
        public static int iRepeatTry = 0;
        public static int iWritePkgNum = 0;//当前要写入的数据块编号
        public static int iRlyPkgNum = 0;//回复的数据块编号
        public static int iTotalPkgCount = 0;//数据块总数
        public static int iRpyPkgNum = 0;//接收到的确定块编号
        public static Int16 i16CurHead = 0;
        public static FileStream fsSaveFile;//要将网络数据写入的文件流对象
        public static long lCurFilePos;//当前文件流读写位置
        public static bool bThrIsIdle = true;//线程空闲状态指示 
        public static int iUdpBufLen = 0;//要发送的UDP缓冲区长度
        public static int iRecvPkgNum = 0;//接收到的数据包编号
        public static int recvDataLen;
        public static byte[] udpRecvDataBuf;//接收数据的缓冲区
        public static byte[] udpSendDataBuf;//发送数据的缓冲区 
        public static int iWriteFileLen = 0;
        public static Byte[] tmpByte;

        //文件接收线程 
        public static void FileRecvThr()
        {
            int iwr;
            iwr = WaitHandle.WaitAny(meAry);
            iWritePkgNum = -1;
            bool thrNotFinished = true;
            while (thrNotFinished)
            {
                int iWriteFileLen = 0;
                iwr = WaitHandle.WaitAny(meAry,1000);//每次循环要执行的的同步等待 
                switch (iwr)
                {
                    case 0://0表示用户要求线程结束
                        strMsg = "线程被结束";
                        SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                        thrNotFinished = false;//结束循环 
                        break;
                    case 1://收到UDP数据
                        mePkgCome.Reset();//消费本次数据包，重置事件对象 
                        //解析接收的UDP数据包
                        //数据包类型
                        iPkgType             = BitConverter.ToInt32(udpRecvDataBuf,0);
                        iRecvPkgNum     = BitConverter.ToInt32(udpRecvDataBuf, 4);
                        //获取数据块长度，设置文件写入长度
                        iWriteFileLen      = BitConverter.ToInt32(udpRecvDataBuf, 8);
                        if (iPkgType == 101)
                        {//收到文件传送请求包
                            if (iWritePkgNum == -1)
                            {
                                iWritePkgNum = 0;
                                iTotalPkgCount = iRecvPkgNum;
                                int iFileNameLen = BitConverter.ToInt32(udpRecvDataBuf, 8);
                                string strRecvFileName = Encoding.UTF8.GetString(udpRecvDataBuf, 12, iFileNameLen);
                                strSaveFileName = @"d:\" + strRecvFileName;
                                fsSaveFile = new FileStream(strSaveFileName, FileMode.Create, FileAccess.Write);
                                strMsg = string.Format("正在接收文件[{0}]", strRecvFileName);
                                SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                                SendMessage(wndHandle, FILETRAN_SETPROG, 0, iRecvPkgNum);
                            }
                            //忽略重复的请求包 
                        }
                        else { 
                            if (iRecvPkgNum == (iWritePkgNum + 1))
                            {//检查数据包，编号不重复时才写入文件
                                iWritePkgNum = iRecvPkgNum;
                                fsSaveFile.Write(udpRecvDataBuf, 12, iWriteFileLen);
                                SendMessage(wndHandle, FILETRAN_UPTEXT, 100, iWritePkgNum);
                                //更新当前进度
                                SendMessage(wndHandle, FILETRAN_UPPROG, 100, iRecvPkgNum);
                                if ((iWritePkgNum == (iTotalPkgCount)) && fsSaveFile.CanWrite)
                                {//所有块接收完成，关闭文件流
                                    strMsg = "文件传输成功";
                                    SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                                    fsSaveFile.Flush();
                                    fsSaveFile.Close();
                                    fsSaveFile.Dispose();
                                    //线程进入空闲状态
                                    bThrIsIdle = true;
                                }   
                            }
                        }
                        //发送回复包，iCurPkgNum指示已确认的数据块编号，值为0代表准备接收 
                        tmpByte = BitConverter.GetBytes((int)201);
                        Array.Copy(tmpByte,0,udpSendDataBuf,0,4);
                        tmpByte = BitConverter.GetBytes(iWritePkgNum);
                        Array.Copy(tmpByte,0,udpSendDataBuf,4,4);
                        skUdpSend.SendTo(udpSendDataBuf, 8, SocketFlags.None, remoteIPEp); 
                        break;
                } 
            }//总事件循环 
        }

        public static void UdpReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                EndPoint tmpRemoteEp = (EndPoint)remoteIPEp;
                //获取远端IP地址，并设置端口值，准备回复
                iUdpRecvPkgLen = skUdpRecv.EndReceiveFrom(ar, ref tmpRemoteEp);
                remoteIPEp = (IPEndPoint)tmpRemoteEp;
                remoteIPEp.Port=UDPSEND_PORT;
                mePkgCome.Set();
                skUdpRecv.BeginReceiveFrom(udpRecvDataBuf, 0, 1024,
                SocketFlags.None, ref remoteEp, UdpReceiveCallBack, new object());
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            wndHandle = this.Handle;
            InitData(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            meEndThr.Set(); 
        }
    }
}
