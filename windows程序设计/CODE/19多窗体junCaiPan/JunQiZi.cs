using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//每个棋子信息
namespace junCaiPan
{
    public class JunQiZi
    {
        public byte sbXpos;//在棋盘上位置X
        public byte sbYpos;//在棋盘上位置Y 
        public byte sbRank;
        // 0:司令，1:军长，2:师长A，3:师长B，4:旅长A，5:旅长B，6:团长A，7:团长B，
        //8:营长A，9:营长B，10:连长A，11:连长B，12:连长C，13:排长A，14:排长B，15:排长C，
        //16:工兵A，17:工兵B，18:工兵C，19:地雷A，20:地雷B，21:地雷C，22:炸弹A，23:炸弹B，
        //24:军旗，25:用于标识对方棋子级别未知 
        public byte sbPicIndex;
        // 0:红司令  ，1:红军长，  2:红师长，  3:红旅长，  4:红团长， 5:红营长，  6:红连长，  
        //7:红排长，  8:红工兵，  9:红地雷，  10:红炸弹，11:红军旗 ，12:红未知
        // 13:黑司令，14:黑军长，15:黑师长，16:黑旅长，17:黑团长，18:黑营长，
        //19:黑连长，20:黑排长，21:黑工兵，22:黑地雷，23:黑炸弹，24:黑军旗 ，25:黑未知
        public byte sbSide;//0:红方　1:黑方
    }
}
