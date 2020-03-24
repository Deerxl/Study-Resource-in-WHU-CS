using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*创建日期 :2014.05.11
 *修改日期 :2014.05.16
 *类名称    :TxtMixPic
 * 类说明   :文本与图片混和
 */
namespace ChuJuan1
{
    public class TxtMixPic
    {
        public string strChengFen;//数据成分，指示数据属题目，答案，或者知识点。
        public string strDataLeiXing;//数据类型，指明obj部分保存的是文本还是图像
        public object objData;//数据内容(文本或图像)
    }
}
