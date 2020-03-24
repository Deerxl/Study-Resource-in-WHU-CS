using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*创建日期 :2014.05.11
 *修改日期 :2014.06.02
 *类名称    :FenZhi
 * 类说明   :分值核算
 */
namespace ChuJuan1
{
    public static class FenZhi
    {
        public static ArrayList alTi;
        public static int iTotalScore;

        public static Dictionary<string, int> dictTiXingFenzhi =
            new Dictionary<string, int>();
        public static Dictionary<string, int> dictDatiFenzhi =
            new Dictionary<string, int>();

        //分值计算
        public static void ScoreCalculate()
        {  //检查分值是否合理
            string[] Keys = dictTiXingFenzhi.Keys.ToArray();
            for (int i = 0; i < Keys.Length; i++)
            {
                if ((dictTiXingFenzhi[Keys[i]] < 0) || (dictTiXingFenzhi[Keys[i]] > 20))
                {
                    MessageBox.Show("题目分值不合理!");
                    return;
                }
            }


            //分别计算大题分值与总分分值

            //各题型分值清0
            for (int i = 0; i < Keys.Length; i++)
            {
                dictDatiFenzhi[Keys[i]] = 0;
            }
            //各题型分值求和
            for (int i = 0; i < alTi.Count; i++)
            {
                Kaoti oneTi = (Kaoti)alTi[i];
                dictDatiFenzhi[oneTi.strTiXing] += dictTiXingFenzhi[oneTi.strTiXing];

            }
            //计算总分
            iTotalScore = 0;
            //各题型分值清0
            for (int i = 0; i < Keys.Length; i++)
            {
                iTotalScore += dictDatiFenzhi[Keys[i]];
            }
        }
        //设置题类型
        public static void SetTiVal(string strType, int iValue)
        {
            dictTiXingFenzhi[strType] = iValue;
        }
    }

}
