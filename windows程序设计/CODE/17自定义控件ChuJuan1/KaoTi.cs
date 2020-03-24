using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*创建日期 :2014.05.11
 *修改日期 :2014.05.17
 *类名称    :Kaoti
 * 类说明   :单个考题类,可生成多个实例
 */
namespace ChuJuan1
{
    public class Kaoti
    {
        //制题时间
        public DateTime dtCreateTime;
        //考题编号，用于内部标识
        public int iTiNum;
        //题型名称
        public string strTiXing;
        //内容=题目+答案+复习点 
        public ArrayList alNeiRong;//内容是TxtMixPic数组 
        //alNeiRong的第0个元素内容必须是文本
        public Kaoti(int iIndex)
        {//使用编号初始化一个新考题实例
            iTiNum = iIndex;
        }
    }
}
