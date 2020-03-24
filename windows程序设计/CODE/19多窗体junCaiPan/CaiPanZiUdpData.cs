using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//客户端与裁判端的UDP通信数据包 
namespace junCaiPan
{
    public class CaiPanZiUdpData
    {
        //所有的通信信息由字段ZiUdpData的字段表示
        public byte iAction;
        public byte iSideCode;//0:红方    1:黑方
        //iAction=0:客户端尝试连接裁判端 
        //成功iResult=0   或失败{红黑已选}:iResult=1 失败{红黑已满}:iResult=2 
        //iAction=2:客户端断开与服务端通讯
        //iAction=3:裁判指定客户端红方或黑方，双方开始布局
        //iAction=4:客户端通知裁判布子信息
        //iAction=5:裁判通知开始对战 
        //iAction=6:走子 
        //iAction=7:碰子 
        //iAction=8:裁判通知客户端对方走子信息
        //iAction=9:裁判通知客户端对方碰子信息:吃掉对方，被对方吃掉，同归于尽
        //iAction=10:裁判通知客户端比赛结果{胜利或失利} 胜利：iResult=1,失利：iResult=0 
        public byte iResult; 
        public string sSideName;//对战端给自己的命名
        public int iUdpPort;//自己的UDP端口号 
        public byte ziRank;//棋子代码
        // 0:司令，1:军长，2:师长A，3:师长B，4:旅长A，5:旅长B，6:团长A，7:团长B，
        //8:营长A，9:营长B，10:连长A，11:连长B，12:连长C，13:排长A，14:排长B，15:排长C，
        //16:工兵A，17:工兵B，18:工兵C，19:地雷A，20:地雷B，21:地雷C，22:炸弹A，23:炸弹B，24:军旗
        public int isourcePosX, isourcePosY, idestPosY, idestPosX;//
        public int iPengResult;//碰子信息
        public int iBufLen;//总缓冲区字节长度
        public void Data2Buf(ref byte[] DataBuf)
        {
            int iBufLen = 0;
            byte[] tmpBuf;
            //iAction的值是所有操作都具有的，且放在最前
            DataBuf[0] = iAction;
            switch (iAction)
            {
                case 0://iAction=0:客户端尝试连接裁判端
                    //接收UDP的端口值
                    tmpBuf = BitConverter.GetBytes(iUdpPort);
                    Array.Copy(tmpBuf, 0, DataBuf, 1, 4); 
                    tmpBuf = Encoding.UTF8.GetBytes(sSideName);
                    iBufLen = tmpBuf.Length;
                    //字符串字节值 
                    Array.Copy(tmpBuf, 0, DataBuf, 9, iBufLen);
                    //字符串总长字节值 
                    tmpBuf = BitConverter.GetBytes(iBufLen);
                    Array.Copy(tmpBuf, 0, DataBuf, 5, 4);
                    break;
                case 1://iAction=1:   裁判回复客户端连接结果 
                    DataBuf[0] = iAction; 
                    break;
                #region 4:客户端通知裁判布局信息
                case 4://4:客户端通知裁判布局信息 
                    tmpBuf = BitConverter.GetBytes(iSideCode);
                    Array.Copy(tmpBuf, 0, DataBuf, 4, 4);
                    //将棋子二维数组值转化为缓冲区
                    int iZi = 0, jZi = 0;
                    for (iZi = 6; iZi < 12; iZi++)
                    {
                        for (jZi = 0; jZi < 5; jZi++)
                        {
                            tmpBuf = BitConverter.GetBytes(CaiPanDClass.iBoardQizi[iZi][jZi]);
                            Array.Copy(tmpBuf, 0, DataBuf, (iZi * 5 + jZi) * 4 + 8, 4);
                        }
                    }
                    break;
                #endregion 4:客户端通知裁判布局信息
                case 5://4:裁判通知开始对战 
                    break;
                default:
                    break;
            }
        }


        public static bool bRedSideReady;
        public static bool bBlkSideReady;
        public void Buf2Data(byte[] DataBuf, ref CaiPanZiUdpData ziResult)
        {
            int iBufLen = 0;
            ziResult.iAction = DataBuf[0];
            switch (ziResult.iAction)
            {
                case 0:
                    iUdpPort = BitConverter.ToInt32(DataBuf, 1);
                    iBufLen = BitConverter.ToInt32(DataBuf, 5);
                    ziResult.sSideName = Encoding.UTF8.GetString(DataBuf, 9, iBufLen);
                    break;
                case 1://iAction=1:   裁判回复客户端连接结果
                    iResult = DataBuf[0];
                    break;
                #region 4:客户端通知裁判布局信息
                case 4: //iAction=4:客户端发给裁判的布子信息
                    ziResult.iSideCode = DataBuf[1];
                    //将缓冲区字节值转化为棋子二维数组值
                    byte iZi = 0, jZi = 0; //iZi为行序{0-11} jZi为列序{0-5}  
                    //CaiPanDClass.alRedBlkZi
                    if (ziResult.iSideCode == 0)//0:红方 1:黑方
                    {//红下
                        //将当前棋局信息放入alRedBlkZi集合中
                         for (iZi = 6; iZi < 12; iZi++)
                        {
                            for (jZi = 0; jZi < 5; jZi++)
                            {
                                //前面的2个字节表示iAction与iSideCode
                                CaiPanDClass.iBoardQizi[iZi][jZi] = DataBuf[(iZi-6) * 5 + jZi  + 2]; 
                            }
                        }
                         bRedSideReady = true;
                    }
                    else 
                    {
                        //黑上
                        //将当前棋局信息放入alRedBlkZi集合中
                        for (iZi = 0; iZi < 6; iZi++)
                        {
                            for (jZi = 0; jZi < 5; jZi++)
                            {
                                //前面的2个字节表示iAction与iSideCode
                                //行坐标与列坐标需进行翻转
                                CaiPanDClass.iBoardQizi[5-iZi][4-jZi] = DataBuf[iZi * 5 + jZi  + 2]; 
                            }
                        }
                        bBlkSideReady = true;
                    } 
                    //通知主窗体重绘棋盘信息
                    CaiPanDClass.meReDrawMainWnd.Set();
                    //双方准备好可开战。
                    if ((bRedSideReady)&&(bBlkSideReady))
                    {     }
                    break;
                #endregion 4:客户端通知裁判布局信息
            }
        }


    }


}
