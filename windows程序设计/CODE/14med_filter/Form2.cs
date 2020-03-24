using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace med_filter
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        protected override void DefWndProc(ref Message m)
        {//窗体消息处理重载
            switch (m.Msg)
            {
                case GData.GRAY_FINISHED: 
                    pictureBox1.Image = (Bitmap)Bitmap.FromStream(GData.ms_bmp1);
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
