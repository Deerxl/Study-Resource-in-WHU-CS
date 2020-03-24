using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*创建日期 :2014.05.17
 *修改日期 :2014.05.17
 *类名称    :FrmAddTiXing
 * 类说明   :添加题型的窗体类，用于向题库中添加题型
 */
namespace ChuJuan1
{
    public partial class FrmAddTiXing : Form
    {
        public FrmAddTiXing()
        {
            InitializeComponent();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (KeChengTiKu.alTiXing.Count > 0)
            {
                //DataFrm.frmNewKaoTi.SetSelectTiXing(0);
            }
            
            DataFrm.fmAddTiXing.Hide();
            DataFrm.fmMain.BringToFront();
            DataFrm.fmMain.Show();
        }

        private void AddTiXingBtn_Click(object sender, EventArgs e)
        {
            //试题类型按钮点击处理事件
            KeChengTiKu.strTiXingMingCheng = ((Button)sender).Text;
            KeChengTiKu.alTiXing.Add(KeChengTiKu.strTiXingMingCheng);
            DataFrm.fmMain.FreshTiXingList();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Trim().Length>0)
            {
                KeChengTiKu.strTiXingMingCheng = textBox1.Text.Trim();
                KeChengTiKu.alTiXing.Add(KeChengTiKu.strTiXingMingCheng);
                DataFrm.fmMain.FreshTiXingList();
            }
            
        }

    }
}
