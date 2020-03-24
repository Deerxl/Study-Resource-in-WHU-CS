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

namespace FileSend
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        } 
        

        [DllImport("User32.dll")]
        private static extern int SendMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        int lParam
        );
        public const int FILETRAN_UPPROG = 0x502;
        public const int FILETRAN_UPTEXT = 0x503;
        public const int MAX_REPEAT_TIMES = 3;
        public static string strFileFullName;//要传输的文件名称
        public static string strMsg;//要显示的线程信息
        public static bool fileWaitToSend = false;
        public static ManualResetEvent meEndThr;    //结束线程 0
        public static ManualResetEvent mePkgCome;//数据到达 1
        public static ManualResetEvent meStartSend;//开始发送 2
        public static ManualResetEvent[] meAry; 
        //UDP数据发送端口与接收端口值-文件发送端 
        public const int UDPRECV_PORT = 9095;
        public const int UDPSEND_PORT = 9096; 
        public static Socket skUdpSend;
        public static Socket skUdpRecv;
        public static EndPoint remoteEp;
        public static IPEndPoint remoteIPEp;
        public static int iUdpRecvPkgLen;
        public static IntPtr wndHandle; 
        public static int recvDataLen;
        public static byte[] udpRecvDataBuf;//接收数据的缓冲区
        public static byte[] udpSendDataBuf;//发送数据的缓冲区
        public static int iPkgType;
        public static int iRepeatTry = 0;
        public static int iCurPkgNum = 0;//当前正发送的数据块编号
        public static int iRlyPkgNum = 0;//回复的数据块编号
        public static int iTotalPkgCount = 0;//数据块总数
        public static int iReadFilePkgNum = 0;//数据块总数
        public static int iRpyPkgNum = 0;//接收到的确定块编号
        public static Int32 i16CurHead = 0;
        public static FileStream fsCurFile;//当前要传输的文件流对象
        public static long lCurFilePos;//当前文件流读写位置
        public static bool bThrIsIdle = true;//线程空闲状态指示 
        public static int iUdpBufLen = 0;//要发送的UDP缓冲区长度
        public static IPAddress remoteIPAddr;
        public static bool ipIsOK = false;
        public static FileInfo fiTran;
        public static Byte[] tmpByte;
        public static Byte[] tmpFN;
        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case FILETRAN_UPPROG://更新进度条值
                    progressBar1.Value = (int)m.LParam;
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
            meAry = new ManualResetEvent[3];
            meEndThr = new ManualResetEvent(false);//0
            mePkgCome = new ManualResetEvent(false);//1
            meStartSend = new ManualResetEvent(false);//2
            meAry[0] = meEndThr;
            meAry[1] = mePkgCome;
            meAry[2] = meStartSend; 
            //绑定接收UDP数据回调函数
            udpRecvDataBuf = new byte[1024];
            remoteIPEp = new IPEndPoint(IPAddress.Any, 0); 
            skUdpRecv = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, UDPRECV_PORT);
            skUdpRecv.Bind(iep);
            remoteEp = (EndPoint)remoteIPEp; 
            skUdpRecv.BeginReceiveFrom(udpRecvDataBuf, 0, 1024,
                SocketFlags.None, ref remoteEp, UdpReceiveCallBack, new object()); 
            //创建发送数据Socket对象与数据缓冲区
            udpSendDataBuf = new byte[1024];
            skUdpSend = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp); 
            ThreadStart thrStart = new ThreadStart(FileSendThr);
            Thread thrT = new Thread(thrStart);
            thrT.IsBackground = true;
            thrT.Start();
        } 
        //文件传输线程 
public static void FileSendThr()
{
    int iwr;//同步等待事件的返回值
    bool thrNotFinished = true;
    while (thrNotFinished)//0表示要求线程结束
    {
        int iReadFileLen = 0;
        //每次循环的的同步等待操作
        iwr = WaitHandle.WaitAny(meAry,1000);
        switch (iwr)
        {
            case 0://0表示用户要求线程结束
                strMsg = "线程被结束";
                SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                thrNotFinished = false;//结束循环
                break;
            case 1://收到UDP数据 
                mePkgCome.Reset();//消费本次数据包，即重置事件对象 
                //解析回复数据包 
                //回复的块编号
                iRpyPkgNum = BitConverter.ToInt32(udpRecvDataBuf, 4);
                if (iRpyPkgNum == iTotalPkgCount)
                {
                    //最后数据块发送成功，当前文件传输任务结束
                    strMsg = "文件传输已成功完成";
                    SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                    //更新数据进度
                    SendMessage(wndHandle, FILETRAN_UPPROG, 100, iTotalPkgCount);
                    //关闭当前文件流
                    fsCurFile.Close();
                    fsCurFile.Dispose(); 
                    bThrIsIdle = true;//线程进入空闲状态
                    //重置文件传输开始
                    meStartSend.Reset();
                    break;
                } 
                if (iRpyPkgNum==(iReadFilePkgNum))
                {
                    //根据回复编号发送相应编号的数据块 
                    iCurPkgNum = iRpyPkgNum + 1;
                    iRepeatTry = 0;//重置重发次数，每次超时会递增本变量 
                    //数据缓冲区清零
                    Array.Clear(udpSendDataBuf, 0, 1024);
                    //要发送数据包格式:[Int32]104(非整块数据)+
                    //[int32]块编号(４字节)+[int32]数据字节长度+数据内容(1-1000字节值)  
                    tmpByte = BitConverter.GetBytes((Int32)102);//[Int32]102
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 0, 4);
                    tmpByte = BitConverter.GetBytes(iCurPkgNum);//[int32]块编号(4字节)
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 4, 4);
                    //从文件流中读出数据内容，其长度为1-1000字节 
                    iReadFileLen = fsCurFile.Read(udpSendDataBuf, 4 + 4 + 4, 1000);
                    tmpByte = BitConverter.GetBytes(iReadFileLen);//[int32]数据字节长度(4字节)
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 4 + 4, 4);
                    //发送编号为iCurPkgNum的数据块，共[4+4+4+iReadFileLen]字节
                    iUdpBufLen = 4 + 4 + 4 + iReadFileLen;
                    SendMessage(wndHandle, FILETRAN_UPPROG, 100, iCurPkgNum);  
                    iReadFilePkgNum++;
                    //发出UDP数据包
                    skUdpSend.SendTo(udpSendDataBuf, iUdpBufLen, SocketFlags.None, remoteIPEp);
                }   
                break;
            case 2://发送文件信息请求包，无响应时会重发
                meStartSend.Reset();//重置开始发送事件，
                //发送UDP数据包:[Int32]101+[int32]总块数(4字节)＋
                //[int32]文件名长度(4字节)+文件名字符串(字节值)
                iUdpBufLen = 12+tmpFN.Length;
                skUdpSend.SendTo(udpSendDataBuf, iUdpBufLen, SocketFlags.None, remoteIPEp);
                break;
            case WaitHandle.WaitTimeout://超时
                if (bThrIsIdle)
                {//线程无事可做 
                }
                else
                {//线程有任务
                    iRepeatTry++;//累加重发次数
                //检查重发次数，如果超过指定次数则结束文件传输
                    if (iRepeatTry == MAX_REPEAT_TIMES)
                    {
                        if (meStartSend.WaitOne(1))
                        {//是文件请求
                            strMsg = "文件传输失败";
                        }
                        else
                        { //数据传输
                            strMsg = "文件传输中断"; 
                        } 
                        SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100); 
                        //关闭当前文件流
                        fsCurFile.Close();
                        fsCurFile.Dispose(); 
                        bThrIsIdle = true;//线程进入空闲状态
                        //重置文件传输开始
                        meStartSend.Reset(); 
                    }
                    else
                    {   //重发数据包，即发相同的数据包
                        skUdpSend.SendTo(udpSendDataBuf, iUdpBufLen, 
                            SocketFlags.None, remoteIPEp);
                    }
                }
                break;
        }
    }//线程循环主体 
}//线程体

        public static void UdpReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                EndPoint tmpRemoteEp = (EndPoint)remoteIPEp;
                iUdpRecvPkgLen = skUdpRecv.EndReceiveFrom(ar, ref tmpRemoteEp); 
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
        private void button2_Click(object sender, EventArgs e)
        {
            ipIsOK = IPAddress.TryParse(textBox1.Text, out remoteIPAddr);
            if (!ipIsOK)
            {
                label1.Text = "输入IP信息不正确。";
                textBox1.Focus();
            }
            else
            {
                label1.Text = "IP地址解析OK。";
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (ipIsOK)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    strFileFullName = openFileDialog1.FileName;
                    //fileWaitToSend = true;//此变量是否有用？
                    //打开文件流，设置文件读写位置为0
                    fsCurFile = new FileStream(strFileFullName, FileMode.Open, FileAccess.Read);
                    lCurFilePos = 0L; 
                    //设置远端IP地址与端口
                    remoteIPEp.Address = remoteIPAddr;
                    remoteIPEp.Port = UDPSEND_PORT;  
                    //计算数据包总数
                    fiTran = new FileInfo(strFileFullName); 
                    if ((fiTran.Length % 1000L) == 0)
                    {
                        iTotalPkgCount = (int)(fiTran.Length / 1000L);
                    }
                    else
                    {
                        iTotalPkgCount = (int)(fiTran.Length / 1000L + 1);
                    } 
                    //设置进度条值，以文件块数为显示值。
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = iTotalPkgCount;
                    strMsg = string.Format("正在传送文件[{0}]",fiTran.Name);
                    SendMessage(wndHandle, FILETRAN_UPTEXT, 100, 100);
                    //文件读写块为0
                    iReadFilePkgNum = 0;
                    iRepeatTry = 0;//重发次数重置为0 
                    //数据缓冲区清零
                    Array.Clear(udpSendDataBuf, 0, 1024);
                    //[Int32]101
                    tmpByte = BitConverter.GetBytes((Int32)101);
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 0, 4);
                    //[int32]总块数(4字节)
                    tmpByte = BitConverter.GetBytes(iTotalPkgCount);
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 4, 4);
                    //不包含路径的文件名字符串 
                    tmpFN = Encoding.UTF8.GetBytes(fiTran.Name);
                    tmpByte = BitConverter.GetBytes(tmpFN.Length);
                    //[int32]文件名字节长度(4字节)
                    Array.Copy(tmpByte, 0, udpSendDataBuf, 4 + 4, 4);
                    //[int32]文件名长度(4字节) 
                    Array.Copy(tmpFN, 0, udpSendDataBuf, 4 + 4 + 4, tmpFN.Length);
                    bThrIsIdle = false;//线程进入工作状态
                    //设置文件传送开始事件对象。
                    meStartSend.Set();
                }
            }
            else
            {
                label1.Text = "请设置正确的IP地址";
                textBox1.Focus();
            }
        }//button1按下事件 
        private void button3_Click(object sender, EventArgs e)
        {
            //设置线程终止事件对象。
            meEndThr.Set();
        } 
    }
}
