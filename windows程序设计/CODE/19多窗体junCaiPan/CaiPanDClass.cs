using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace junCaiPan
{
    public static class CaiPanDClass
    {
        public static JunAppContext junContext;
        public static MemoryStream msBK;
        public static ArrayList alPic;
        //窗体要显示的全部内容
        public static Bitmap bmpViewOld;
        public static Bitmap bmpViewNew;
        //所有的25个红子与25个黑子
        public static ArrayList alJunZi;
        //当前棋局信息12行5列;其值表示在所放棋子在集合
        //alJunZi(50个)中的下标， 值为255表示未放子
        public static Byte[][] iBoardQizi = new Byte[12][];
        public static ArrayList alRedBlkZi;



        //初始化棋子资，将用到的所有棋子图片对象放入数组alPic 
        public static void InitPicRes()
        {
            //初始化图片资源 
            alPic = new ArrayList();
            #region  黑子与红子图片数组
            //alPicNames集合中是图片资源文件名
            ArrayList alPicNames = new ArrayList();
            alPicNames.Add("红司令.png");//0
            alPicNames.Add("红军长.png");//1
            alPicNames.Add("红师长.png");//2
            alPicNames.Add("红旅长.png");//3
            alPicNames.Add("红团长.png");//4
            alPicNames.Add("红营长.png");//5
            alPicNames.Add("红连长.png");//6
            alPicNames.Add("红排长.png");//7
            alPicNames.Add("红工兵.png");//8
            alPicNames.Add("红地雷.png");//9
            alPicNames.Add("红炸弹.png");//10
            alPicNames.Add("红军旗.png");//11
            alPicNames.Add("红未知.png");//12
            alPicNames.Add("黑司令.png");//13
            alPicNames.Add("黑军长.png");//14
            alPicNames.Add("黑师长.png");//15
            alPicNames.Add("黑旅长.png");//16
            alPicNames.Add("黑团长.png");//17
            alPicNames.Add("黑营长.png");//18
            alPicNames.Add("黑连长.png");//19
            alPicNames.Add("黑排长.png");//20
            alPicNames.Add("黑工兵.png");//21
            alPicNames.Add("黑地雷.png");//22
            alPicNames.Add("黑炸弹.png");//23
            alPicNames.Add("黑军旗.png");//24
            alPicNames.Add("黑未知.png");//25
            alPicNames.Add("BGMain.png");//背景图 26 Main 
            alPicNames.Add("selectMain.png");//选择框 27 Main 
            #endregion　黑子与红子图片数组

            #region 读入图片资源 
            msBK = new MemoryStream(3000000);
            byte[] msBuf;
            msBuf = msBK.GetBuffer();
            FileStream fs;
            FileInfo finPic;
            MemoryStream msBmp;
            Bitmap oneBmp;
            int picByteCount;
            for (int i = 0; i < alPicNames.Count; i++)
            {
                string onePicName = ".\\img\\" + alPicNames[i].ToString();
                finPic = new FileInfo(onePicName);
                fs = new FileStream(onePicName, FileMode.Open, FileAccess.Read);
                picByteCount = fs.Read(msBuf, 0, 2000000);

                fs.Close();
                fs.Dispose();
                msBmp = new MemoryStream(msBuf, 0, picByteCount);
                oneBmp = new Bitmap(msBmp);
                alPic.Add(oneBmp);
            }
            #endregion 读入图片资源 
            picBackGround = (Bitmap)CaiPanDClass.alPic[26];
            picSelect = (Bitmap)CaiPanDClass.alPic[27]; 
        }
        

        //初始化棋子资，将用到的所有棋子对象放入数组alJunZi 
        public static void InitBoardQiZi()
        { 
            alJunZi = new ArrayList();
            JunQiZi oneQizi;
            #region 红方棋子
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 0;//司令
            oneQizi.sbPicIndex = 0;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 1;//军长
            oneQizi.sbPicIndex = 1;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 2;//师长1
            oneQizi.sbPicIndex = 2;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 2;//师长2
            oneQizi.sbPicIndex = 2;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 3;//旅长1
            oneQizi.sbPicIndex = 3;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 3;//旅长2
            oneQizi.sbPicIndex = 3;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 4;//团长1
            oneQizi.sbPicIndex = 4;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 4;//团长2
            oneQizi.sbPicIndex = 4;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 5;//营长1
            oneQizi.sbPicIndex = 5;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 5;//营长2
            oneQizi.sbPicIndex = 5;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//连长1
            oneQizi.sbPicIndex = 6;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//连长2
            oneQizi.sbPicIndex = 6;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//连长3
            oneQizi.sbPicIndex = 6;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//排长1
            oneQizi.sbPicIndex = 7;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//排长2
            oneQizi.sbPicIndex = 7;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//排长3
            oneQizi.sbPicIndex = 7;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//工兵1
            oneQizi.sbPicIndex = 8;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//工兵2
            oneQizi.sbPicIndex = 8;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//工兵3
            oneQizi.sbPicIndex = 8;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//地雷1
            oneQizi.sbPicIndex = 9;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//地雷2
            oneQizi.sbPicIndex = 9;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//地雷3
            oneQizi.sbPicIndex = 9;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 10;//炸弹1
            oneQizi.sbPicIndex = 10;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 10;//炸弹2
            oneQizi.sbPicIndex = 10;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 11;//红军旗
            oneQizi.sbPicIndex = 11;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 100;//红未知
            oneQizi.sbPicIndex = 12;
            oneQizi.sbSide = 0;//红方
            alJunZi.Add(oneQizi); 
            #endregion 红方棋子

            #region 黑方棋子
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 0;//黑司令
            oneQizi.sbPicIndex = 13;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 1;//黑军长
            oneQizi.sbPicIndex = 14;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 2;//黑师长1
            oneQizi.sbPicIndex = 15;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 2;//黑师长2
            oneQizi.sbPicIndex = 15;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 3;//黑旅长1
            oneQizi.sbPicIndex = 16;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 3;//黑旅长2
            oneQizi.sbPicIndex = 16;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 4;//黑团长1
            oneQizi.sbPicIndex = 17;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 4;//黑团长2
            oneQizi.sbPicIndex = 17;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 5;//黑营长1
            oneQizi.sbPicIndex = 18;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 5;//黑营长2
            oneQizi.sbPicIndex = 18;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//黑连长1
            oneQizi.sbPicIndex = 19;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//黑连长2
            oneQizi.sbPicIndex = 19;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 6;//黑连长3
            oneQizi.sbPicIndex = 19;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//黑排长1
            oneQizi.sbPicIndex = 20;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//黑排长2
            oneQizi.sbPicIndex = 20;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 7;//黑排长3
            oneQizi.sbPicIndex = 20;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//黑工兵1
            oneQizi.sbPicIndex = 21;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);

            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//黑工兵2
            oneQizi.sbPicIndex = 21;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 8;//黑工兵3
            oneQizi.sbPicIndex = 21;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//黑地雷1
            oneQizi.sbPicIndex = 22;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//黑地雷2
            oneQizi.sbPicIndex = 22;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 9;//黑地雷3
            oneQizi.sbPicIndex = 22;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 10;//黑炸弹1
            oneQizi.sbPicIndex = 23;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 10;//黑炸弹2
            oneQizi.sbPicIndex = 23;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 11;//黑军旗
            oneQizi.sbPicIndex = 24;
            oneQizi.sbSide = 1;//黑方
            alJunZi.Add(oneQizi);
            oneQizi = new JunQiZi();
            oneQizi.sbRank = 12;//黑未知
            oneQizi.sbPicIndex = 25;
            oneQizi.sbSide = 1;//黑未知
            alJunZi.Add(oneQizi);
            #endregion 黑方棋子
             
            for (int j = 0; j < 12; j++)
            {
                iBoardQizi[j] = new Byte[5];
                for (int i = 0; i < 5; i++)
                {
                    iBoardQizi[j][i] = 255;
                }
            }
            dataBuf = new byte[500];

            alRedBlkZi = new ArrayList(); 
        }



        //棋子数据缓冲区
        public static byte[] dataBuf;
        public static byte[] tmpBuf;
        //棋子信息转换为字节缓冲区
        public static void BoardQiZi2Buf()
        {
            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    tmpBuf = BitConverter.GetBytes(iBoardQizi[j][i]);
                    Array.Copy(tmpBuf, 0, dataBuf, 20 * j + i * 4, 4);
                }
            }

        }
        //字节缓冲区转换为棋子信息
        public static void Buf2BoardQiZi()
        {
            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    //iBoardQizi[j][i] = dataBuf[20 * j + i * 4];
                    iBoardQizi[j][i] = dataBuf[2];
                }
            }
        }

        //窗体信息更新的自定义消息
        public static string sOneTextInfo;
        public const int WM_UPDATEINFOA=0x501;
        public const int WM_UPDATEINFOB = 0x502; 
        public const int WM_PAINT = 0x0f;
        public static ManualResetEvent meTermiNateDraw;
        public static ManualResetEvent meReDrawMainWnd;
        public static ManualResetEvent[] meArray;
        public static IntPtr mainWndPtr;
        //初始化二维的坐标数组
        public static int[] xPos = new int[5] { 7, 134, 264, 388, 525 };
        public static int[] yPos = new int[12] { 25, 82, 137, 196, 256, 313, 451, 510, 567, 626, 680, 738 };
        public static void index2PointXY(int indexX, int indexY, out int xpos, out int ypos)
        {
            xpos = xPos[indexX];
            ypos = yPos[indexY];
            return;
        } 
        public static int xpPos;
        public static int ypPos;
        public static Bitmap onePicMain; 
        public static Bitmap picBackGround;
        public static Bitmap picSelect;
        public static int iCurSelect;
        public static int iClickState; 
         


        //绘制完整棋盘棋子信息
        public static void DrawBoard()
        { 
            bmpViewNew = new Bitmap(600, 800);
            Graphics gDrawViewMain = Graphics.FromImage(bmpViewNew);  
            int iWaitResult;
            meArray = new ManualResetEvent[2];
            meTermiNateDraw = new ManualResetEvent(false);
            meReDrawMainWnd = new ManualResetEvent(false);
            meArray[0] = meTermiNateDraw;
            meArray[1] = meReDrawMainWnd;
            iWaitResult = WaitHandle.WaitAny(meArray, 500); 
            DateTime oldNow = DateTime.Now;
            //DateTime newNow;
            while (iWaitResult != 0)
            {
                switch (iWaitResult)
                {
                    case 1://重绘棋盘
                        //重置事件状态，消费本次事件
                        meReDrawMainWnd.Reset();
                        //棋盘
                        gDrawViewMain.DrawImage(picBackGround, 0, 0, 600, 800);
                        //绘制所有棋子
                        for (int iY = 0; iY < 12; iY++)
                        {
                            for (int iX = 0; iX < 5; iX++)
                            {
                                //有放子的情况下，绘制相应棋子
                                if (iBoardQizi[iY][iX] != 255)
                                {
                                    JunQiZi oneJunQi = (JunQiZi)alJunZi[iBoardQizi[iY][iX]];
                                    onePicMain = (Bitmap)CaiPanDClass.alPic[oneJunQi.sbPicIndex];
                                    index2PointXY(iX, iY, out xpPos, out ypPos);
                                    gDrawViewMain.DrawImage(onePicMain, xpPos + 4, ypPos + 6, 56, 26);
                                }
                            }
                        } 
                        //向主窗体发送消息，使用重绘制界面
                        NativeMethods.SendMessage(mainWndPtr, WM_PAINT, 0, 0);
                        break;
                    default:
                        break;
                }
                //继续下次循环
                iWaitResult = WaitHandle.WaitAny(meArray, 500);
            }
        }



        public const int CLIENTA_PORT = 0x678;   //A客户端UDP接收端口值
        public const int CLIENTB_PORT = 0x679;   //B客户端UDP接收端口值
        public const int CAIPAN_PORT = 0x680; //裁判端UDP接收端口值    
        public static bool bClientAIsRed = true;
        public static int iClientCount;//客户端数量
        public static IPAddress clientAIP;//客户A，先登录裁判端的为客户A
        public static IPAddress clientBIP;//客户B，后登录裁判端的为客户B 
        public static IPEndPoint clientIPEPA;//客户A的IP地址与端口
        public static IPEndPoint clientIPEPB;//客户A的IP地址与端口 
        public static string  clientAName;//客户A名称
        public static string clientBName;//客户B名称
        public static int iclientAPort;//客户A端口
        public static int iclientBPort;//客户B端口


        #region  获取本机IP地址        
        public static IPAddress localIP;
        public static string strLocalAddr;
        //获取本机IP地址
        public static void FetchLocalIPAddr()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] netWkItfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface oneNetWkitf in netWkItfaces)
            {
                IPInterfaceProperties ipitfpops = oneNetWkitf.GetIPProperties();
                UnicastIPAddressInformationCollection uipAdInfoCo = ipitfpops.UnicastAddresses;
                foreach (UnicastIPAddressInformation oneUipAdInfo in uipAdInfoCo)
                {
                    if (oneUipAdInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    { 
                        if(ipitfpops.GatewayAddresses.Count>0)
                        {
                            strLocalAddr = oneUipAdInfo.Address.ToString();
                        }
                    }
                }
            }
            if (!IPAddress.TryParse(strLocalAddr, out localIP))
            {
                MessageBox.Show("裁判端获取本机IP地址异常。");
            }
        }
        #endregion 获取本机IP地址 
    }
}
