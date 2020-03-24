using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace rens
{
    public partial class F_main : Form
    {
        public F_main()
        {
            InitializeComponent();
        }
          
        private void 全部菜单点击处理_Click(object sender, EventArgs e)
        {
            Form cur_frm;
            if (GData.frm_dict.TryGetValue(sender.ToString().Trim(), out  cur_frm))
            {
                cur_frm.Show();
            }
        } 

        private void F_main_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ArrayList menu_al=new ArrayList();//用来保存所有的菜单项
            ArrayList node_al = new ArrayList();//用来保存所有的树节点
            ToolStripDropDownItem tsdi_one;
            TreeNode oneNode,twoNode;
            for (int i = 0; i < menuStrip1.Items.Count; i++) //遍历MenuStrip组件中的一级菜单项
            {
                tsdi_one = (ToolStripDropDownItem)menuStrip1.Items[i];
                oneNode = treeView1.Nodes.Add(tsdi_one.Text);
                menu_al.Add(tsdi_one);                
                node_al.Add(oneNode);
            }
            while(menu_al.Count>0)
            {
                tsdi_one=(ToolStripDropDownItem)menu_al[0];
                oneNode = (TreeNode)node_al[0];
                //如果存在子节点，则将子节点加入到队列中
                foreach (ToolStripDropDownItem tsd_item in tsdi_one.DropDownItems)
                {
                    twoNode = oneNode.Nodes.Add(tsd_item.Text);
                    menu_al.Add(tsd_item);     
                    node_al.Add(twoNode);
                }
                menu_al.RemoveAt(0);
                node_al.RemoveAt(0); 
            } 
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Text.Trim() == "退出系统")   //如果当前节点的文本为“退出系统”
            {
                this.Close();//主窗体退出，整个程序结束
            }
            else {
                Form cur_frm;
                if (GData.frm_dict.TryGetValue(e.Node.Text.Trim(),out  cur_frm))
                {
                    cur_frm.Show();
                }
                else
                {
                    MessageBox.Show("对不起，找不到菜单的对应窗体");
                }
                
            }
        }

        private void 退出系统ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
