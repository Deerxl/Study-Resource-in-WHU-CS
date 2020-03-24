using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//裁判端UDP传输线程
namespace junCaiPan
{
    public class UdpCaipanTran
    {   //Udp接收缓冲区
        public static byte[] udpCaiPanRecvDataBuf;
        //Udp发送缓冲区
        public static byte[] udpCaiPanSendDataBuf;
        public static ManualResetEvent meTermiNateCaiPanUdp;
        public static ManualResetEvent meCaiPanSendUdpData;
        public static ManualResetEvent meCaiPanGotUdpData;
        public static ManualResetEvent meCaiPanBeginWarUdpData;//裁判通知双方开始对弈
        public static ManualResetEvent meCaiPanSetRedAndBlack;//裁判设置红黑方
        public static ManualResetEvent[] meUdpCaiPanArray;
        public static EndPoint clientRemoteEP;
        public static IPEndPoint cRmtEpRed;
        public static IPEndPoint cRmtEpBlk;


        //裁判端数据双向传输线程
        public static void CaiPanUdpTranThread()
        {
            udpCaiPanRecvDataBuf = new byte[1024];
            udpCaiPanSendDataBuf = new byte[1024];
            sockUdpCaiPanRecv = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp);
            sockUdpCaiPanSend = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp);
            clientRemoteEP = new IPEndPoint(IPAddress.Any, 0);//remoteEP为EndPoint类型
            IPEndPoint ipe4bind = new IPEndPoint(IPAddress.Any, 
                CaiPanDClass.CAIPAN_PORT);//裁判端的端口绑定
            sockUdpCaiPanRecv.Bind(ipe4bind);
            sockUdpCaiPanRecv.BeginReceiveFrom(udpCaiPanRecvDataBuf, 
                0, 1024, SocketFlags.None,
                ref clientRemoteEP, CaiPanUdpCallBack, new object());
            //创建事件对象
            int iWaitResult;
            meUdpCaiPanArray = new ManualResetEvent[5];
            meTermiNateCaiPanUdp = new ManualResetEvent(false);
            meCaiPanSendUdpData = new ManualResetEvent(false);
            meCaiPanGotUdpData = new ManualResetEvent(false);//收到UDP数据事件
            meCaiPanBeginWarUdpData = new ManualResetEvent(false);//裁判通知双方开始对弈
            meCaiPanSetRedAndBlack = new ManualResetEvent(false);//裁判通知双方开始对弈
            meUdpCaiPanArray[0] = meTermiNateCaiPanUdp;
            meUdpCaiPanArray[1] = meCaiPanSendUdpData;
            meUdpCaiPanArray[2] = meCaiPanGotUdpData;
            meUdpCaiPanArray[3] = meCaiPanBeginWarUdpData;
            meUdpCaiPanArray[4] = meCaiPanSetRedAndBlack;

            iWaitResult = WaitHandle.WaitAny(meUdpCaiPanArray, 5000);
            CaiPanZiUdpData oneZiData;
            while (iWaitResult != 0)
            {
                switch (iWaitResult)
                {
                    case 1:
                        //通知服务端向红方和黑方回复开战，  
                        oneZiData = new CaiPanZiUdpData();
                        //iAction=5:裁判通知开始对战 
                        oneZiData.iAction = 5;
                        oneZiData.iResult = 1;//红方开战，请红方先走
                        oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                        //向红方客户端发送准备开战。
                        sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, cRmtEpRed);
                        oneZiData.iResult = 2;//黑方开局，黑方不走
                        oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                        //向绿方客户端发送准备开战。
                        sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, cRmtEpBlk); 
                        break;
                    case 2://收到UDP数据
                        oneZiData = new CaiPanZiUdpData();
                        oneZiData.Buf2Data(udpCaiPanRecvDataBuf, ref oneZiData);
                        switch (oneZiData.iAction)
                        {
                            case 0://iAction=0:客户端红方或黑方尝试连接裁判端  
                                IPEndPoint cRmtEp; 
                                if (oneZiData.iSideCode == 0)
                                { //iSideCode=0代表是红方
                                    cRmtEpRed = new IPEndPoint(((IPEndPoint)clientRemoteEP).Address, oneZiData.iUdpPort);
                                    CaiPanDClass.iclientAPort = oneZiData.iUdpPort;
                                    CaiPanDClass.clientAName = oneZiData.sSideName;
                                    CaiPanDClass.clientIPEPA = new IPEndPoint(((IPEndPoint)clientRemoteEP).Address, CaiPanDClass.iclientAPort);
                                    CaiPanDClass.sOneTextInfo = string.Format("选手A：{0}", oneZiData.sSideName);
                                    NativeMethods.SendMessage(CaiPanDClass.mainWndPtr, CaiPanDClass.WM_UPDATEINFOA, 0, 0);
                                    cRmtEp = cRmtEpRed;
                                }
                                else {
                                    cRmtEpBlk = new IPEndPoint(((IPEndPoint)clientRemoteEP).Address, oneZiData.iUdpPort);
                                    CaiPanDClass.iclientBPort = oneZiData.iUdpPort;
                                    CaiPanDClass.clientBName = oneZiData.sSideName;
                                    CaiPanDClass.clientIPEPB = new IPEndPoint(((IPEndPoint)clientRemoteEP).Address, CaiPanDClass.iclientBPort); 
                                    CaiPanDClass.sOneTextInfo = string.Format("选手B：{0}", oneZiData.sSideName);
                                    NativeMethods.SendMessage(CaiPanDClass.mainWndPtr, CaiPanDClass.WM_UPDATEINFOB, 0, 0);
                                    cRmtEp = cRmtEpBlk;
                                } 
                                //收到客户端连接马上进行回复，表示连接成功
                                oneZiData = new CaiPanZiUdpData();
                                oneZiData.iAction = 1;//iAction=1表示连接成功
                                oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                                //分别向客户端A或客户端B发送连接成功数据包。
                                sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, cRmtEp); 
                                break;
                            case 4://iAction=4:客户端发来布子信息 
                                break; 
                        }
                        //收到选手连接信息后
                        meCaiPanGotUdpData.Reset();//事件完成后，重置其状态为无效
                        break;

                    case 3://事件序号3：裁判通知双方开始对弈
                        oneZiData = new CaiPanZiUdpData();
                        oneZiData.iAction = 3;
                        oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                        //分别向客户端A与客户端B发送开始对弈数据包。
                        sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, CaiPanDClass.clientIPEPA); 
                        meCaiPanBeginWarUdpData.Reset();//通知事件到达，重置事件状态
                        break;
                    case 4://事件下标为4，设置红黑方
                        oneZiData = new CaiPanZiUdpData();
                        if (CaiPanDClass.bClientAIsRed)
                        {
                            //向客户端A发红色标记
                            oneZiData.iAction = 2;
                            oneZiData.iSideCode = 1;
                            oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                            //向客户端A发送 红黑设置信息。
                            sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, CaiPanDClass.clientIPEPA);
                            //向客户端B发黑色标记
                            oneZiData.iAction = 2;
                            oneZiData.iSideCode = 2;
                            oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                            //向客户端B发送 红黑设置信息。
                            sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, CaiPanDClass.clientIPEPB);
                        }
                        else
                        {
                            //向客户端A发黑色标记
                            oneZiData.iAction = 2;
                            oneZiData.iSideCode = 2;
                            oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                            //向客户端A发送 红黑设置信息。
                            sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, CaiPanDClass.clientIPEPA);
                            //向客户端B发红色标记
                            oneZiData.iAction = 2;
                            oneZiData.iSideCode = 1;
                            oneZiData.Data2Buf(ref udpCaiPanSendDataBuf);
                            //向客户端B发送 红黑设置信息。
                            sockUdpCaiPanSend.SendTo(udpCaiPanSendDataBuf, 1024, SocketFlags.None, CaiPanDClass.clientIPEPB);
                        }
                        oneZiData.iAction = 3;
                        //通知事件到达，重置事件状态
                        meCaiPanSetRedAndBlack.Reset();
                        break;
                }
                iWaitResult = WaitHandle.WaitAny(meUdpCaiPanArray, 5000);
            }
            //结束Udp对象
            sockUdpCaiPanRecv.Close();
            sockUdpCaiPanSend.Close();
        }




        public static Socket sockUdpCaiPanRecv, sockUdpCaiPanSend;
        public static int iUdpRecvDataLen;
        //裁判端UDP数据接收回调函数
        public static void CaiPanUdpCallBack(IAsyncResult ar)
        {
            try
            {
                EndPoint tmpRemotEP = (EndPoint)clientRemoteEP;
                iUdpRecvDataLen = sockUdpCaiPanRecv.EndReceiveFrom(ar, ref tmpRemotEP);
                clientRemoteEP = (IPEndPoint)tmpRemotEP;

                //设置UDP数据到达事件有效
                meCaiPanGotUdpData.Set();
                //继续接收后续UDP数据
                sockUdpCaiPanRecv.BeginReceiveFrom(udpCaiPanRecvDataBuf, 0, 1024, SocketFlags.None,
                ref clientRemoteEP, CaiPanUdpCallBack, new object());
            }
            catch (SocketException se)
            {
                MessageBox.Show("裁判端UDP接收异常" + se.Message);
            } 
        }
    }
}