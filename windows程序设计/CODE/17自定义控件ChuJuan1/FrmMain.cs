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
 *类名称    :FrmMain
 * 类说明   :主窗体类，功能：题库管理与编辑
 */
namespace ChuJuan1
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        public void FreshTiXingList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < KeChengTiKu.alTiXing.Count;i++ )
            {
                listBox1.Items.Add(KeChengTiKu.alTiXing[i]);
            }
            FreshKaoTiTree();
        }
        public void FreshKaoTiTree()
        {
            //清除显示所有树节点
            kaoTiTree.Nodes.Clear();
            if (KeChengTiKu.strKCMingCheng == null)
            {
                return;
            }

            //树根节点是课程名称
            kaoTiTree.Nodes.Add(KeChengTiKu.strKCMingCheng, KeChengTiKu.strKCMingCheng);
            //一级节点是题型
            for (int i = 0; i < KeChengTiKu.alTiXing.Count; i++)
            {
                kaoTiTree.Nodes[0].Nodes.Add(KeChengTiKu.alTiXing[i].ToString(), KeChengTiKu.alTiXing[i].ToString());
            }
            //二级节点是各类型题目
            for (int i = 0; i < KeChengTiKu.alKaoti.Count; i++)
            {
                Kaoti oneKaoti = (Kaoti)KeChengTiKu.alKaoti[i];
                int iNodeIndex = kaoTiTree.Nodes[0].Nodes.IndexOfKey(oneKaoti.strTiXing);
                TreeNode ParentNode = kaoTiTree.Nodes[0].Nodes[iNodeIndex];
                TxtMixPic theTMP=(TxtMixPic)oneKaoti.alNeiRong[0];
                ParentNode.Nodes.Add(oneKaoti.iTiNum.ToString(), theTMP.objData.ToString());
            }
            kaoTiTree.ExpandAll();
        }
        public void FreshTextPicShow()
        { //将当前考题内容刷新显示
            
        }
        public void SetSelectTiXing(int selectIndex)
        {
            listBox1.SetSelected(selectIndex, true);
        }


        private void 添加题型ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //向树控件添加题型对话框
            DataFrm.fmAddTiXing.BringToFront();
            DataFrm.fmAddTiXing.Show(); 
        }
         

        private void kaoTiTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            MessageBox.Show("NodeMouseDoubleClick");
            if (e.Node.Level == 2)
            {
            }
        }

        private void 添加考题ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (KeChengTiKu.alTiXing.Count < 1)
            {
                MessageBox.Show("需要先添加题型");
                return;
            }

            

            KeChengTiKu.AddNewKaoTi();
            DataFrm.fmEdKaoTi.FreshDataView();
            DataFrm.fmEdKaoTi.BringToFront();
            DataFrm.fmEdKaoTi.Show();
        }

        private void kaoTiTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            MessageBox.Show("NodeMouseClick");
            if (e.Node.Level == 2)
            {
                int iKaoTi = Int32.Parse(e.Node.Name);
                KeChengTiKu.curKaoTi = (Kaoti)KeChengTiKu.alKaoti[iKaoTi];
                FreshTextPicShow();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //新建题库按钮
            kaoTiTree.Nodes.Clear();
            DataFrm.fmNewTiKu.BringToFront();
            DataFrm.fmNewTiKu.Show();
        }
    }
}
