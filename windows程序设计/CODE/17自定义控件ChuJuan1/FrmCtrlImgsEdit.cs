using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*创建日期 :2014.06.01
 *修改日期 :2014.06.01
 *类名称    :FrmCtrlImgsEdit
 * 类说明   :本窗体用于编辑图片列表，保持每个图片自身的尺寸
 */
namespace ChuJuan1
{
    public partial class FrmCtrlImgsEdit : Form
    {
        public FrmCtrlImgsEdit(ArrayList imList)
        {
            InitializeComponent();
            imgList = imList;
        }
        public ArrayList imgList;

        private void button1_Click(object sender, EventArgs e)
        {
            //确定按钮
        }
        //刷新listView的item项目
        private void FreshItems()
        { 
            listView1.Items.Clear();
            for(int i=0;i<imgList.Count;i++)
            {
                listView1.Items.Add("图片" + i.ToString(), i); 
            }
            Invalidate();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap theBMP = new Bitmap(openFileDialog1.FileName);
                imgList.Add(theBMP);
                LVimList.Images.Add(theBMP);
                FreshItems();
                listView1.Invalidate();
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            iSelectedItem = e.ItemIndex;
            Bitmap theBMP=(Bitmap)imgList[e.ItemIndex];
            label3.Text = string.Format("宽：{0}",theBMP.Width);
            label4.Text = string.Format("高：{0}",theBMP.Height);
        }
        private int iSelectedItem=-1;
        private void button6_Click(object sender, EventArgs e)
        {
            if (iSelectedItem!=-1)
             {
                 listView1.Items.RemoveAt(iSelectedItem);
                 imgList.RemoveAt(iSelectedItem);
                 iSelectedItem = -1;
             }
        }

        private void FrmCtrlImgsEdit_Load(object sender, EventArgs e)
        {
            
            FreshItems();
        } 
    }
}
