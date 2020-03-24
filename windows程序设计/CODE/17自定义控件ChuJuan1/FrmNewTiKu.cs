using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChuJuan1
{
    public partial class FrmNewTiKu : Form
    {
        public FrmNewTiKu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //检测用户输入
            if (textBox1.Text.Trim().Length < 1)
                MessageBox.Show("请输入课程名称");



            //创建新题库
            KeChengTiKu.InitKeChengTiKu();
            KeChengTiKu.strKCMingCheng = textBox1.Text.Trim();


            DataFrm.fmMain.Text = "题库名称---" + KeChengTiKu.strKCMingCheng;
            //刷新考题树控件 
            DataFrm.fmNewTiKu.Hide();
            DataFrm.fmMain.FreshKaoTiTree();
            DataFrm.fmMain.BringToFront();
            DataFrm.fmMain.Show();
        }

        private void FrmNewTiKu_FormClosing(object sender, FormClosingEventArgs e)
        {
            DataFrm.fmNewTiKu.Hide();
            e.Cancel = true;
            DataFrm.fmMain.BringToFront();
            DataFrm.fmMain.Show();
        }
    }
}
