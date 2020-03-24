using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Design;
/*创建日期 :2014.06.01
 *修改日期 :2014.06.01
 *类名称    :UserControl1
 * 类说明   :自定义控件，用于显示图片集，其中图片的尺寸可保持原样
 */
namespace ChuJuan1
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
            //减少闪烁
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true); 
        } 
        private int mScrollPos;         //滚动条位置 
        public int ScrollPos 
        {
            get
            {
                return mScrollPos;
            }
            set
            {
                mScrollPos = value;
                Invalidate();
            }
        }

        private ArrayList mImgArray = new ArrayList();
        [Browsable(true)]
        //修改属性编辑调用的窗体对象
        [Editor(typeof(ImageEditUIType), typeof(UITypeEditor))]

        public ArrayList ImageArray
        {
            get
            {
                return mImgArray;
            }
            set
            {
                mImgArray = value;
                Invalidate();
            }
        } 

        //重绘自定义控件
        private void DrawCtl(PaintEventArgs e)
        { 
            SolidBrush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black);
            Rectangle rect = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1); 
              
            //e.Graphics.DrawString(string.Format("滚动位置{0}",mScrollPos), new Font("宋体", 10f), brush, 5, 5);
            brush.Dispose();
             
            //e.Graphics.DrawLine(pen, 1, 1, ClientRectangle.Width - 2, 1);
            //e.Graphics.DrawLine(pen, 1, 1, 1, ClientRectangle.Height - 2);

            int theHeight = 10;
            int totalHeight = 0; 

            for (int i = 0; i < ImageArray.Count; i++)
            {
                Bitmap theBmp = (Bitmap)ImageArray[i]; 
                totalHeight += theBmp.Size.Height + 20;
            }

            for (int i = 0; i < ImageArray.Count; i++)
            {
                Bitmap theBmp = (Bitmap)ImageArray[i];
                Size imSize = theBmp.Size;
                Rectangle destRect = new Rectangle(40, theHeight - mScrollPos * totalHeight / 21, imSize.Width, imSize.Height);
                e.Graphics.DrawImage(theBmp, destRect);
                theHeight += imSize.Height + 20;
            } 
        }
        //重载绘图函数，绘制自定义数据内容
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawCtl(e);
        }


        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            mScrollPos = vScrollBar1.Value ;
            Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmCtrlImgsEdit dlg = new FrmCtrlImgsEdit(mImgArray);
            dlg.imgList.Clear();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                mImgArray = dlg.imgList;
                Invalidate();
            }
        }

        //给控件赋新图片集合值，并更新显示，用于直接更新显示图片集
        public void SetImgArray(ArrayList newImgArray)
        {
            mImgArray = newImgArray;
            Invalidate();
        }


    }
}
