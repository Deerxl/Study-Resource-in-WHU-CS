using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//maingame 用于对整个游戏逻辑的运作
namespace junCaiPan
{
    public static class CaiPanGameMain
    {
        //1.显示棋盘信息,红绿双方棋子
        //2.传递消息
        //3.对接收到的消息进行载定 
        //计算碰子结果
        public static void JudgeCollide(int iRedZi,int iGreenZi,out int iRedResult,out int iGreenResult)
        {
            iRedResult = 1;
            iGreenResult = 1;
        } 
        public static void GameControl()
        { 
            //运行游戏的主逻辑控制
            //1.等待双方准备
            //2.接收客户端走子信息
            //3.转发走子信息给另一方
            //4.判断碰子信息
        } 
    }
}
