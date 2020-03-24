using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*创建日期 :2014.05.18
 *修改日期 :2014.05.18
 *类名称    :FrmEditKaoTi
 * 类说明   :编辑考题窗体类
 * 功能      :新建或编辑当前考题
 */
namespace ChuJuan1
{
    public partial class FrmEditKaoTi : Form
    {
        public FrmEditKaoTi()
        {
            InitializeComponent();
        }
        public void FreshDataView()
        { 
            //显示当前考题内容
            //KeChengTiKu.curKaoTi.alNeiRong;
            listBox1.Items.Clear();
            for (int i = 0; i < KeChengTiKu.alTiXing.Count;i++ )
            {
                listBox1.Items.Add(KeChengTiKu.alTiXing[i]);
            }



            listView1.Items.Clear();
            for (int i = 0; i < KeChengTiKu.curKaoTi.alNeiRong.Count;i++ )
            {
                TxtMixPic curTMP = (TxtMixPic)KeChengTiKu.curKaoTi.alNeiRong[i];
                listView1.Items.Add(curTMP.strChengFen+"-"+curTMP.strDataLeiXing);
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (listBox1.Text.IndexOf("文本")>0)
            {
            }
            if (listBox1.Text.IndexOf("图片") > 0)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FreshDataView();
        }
    }
}
