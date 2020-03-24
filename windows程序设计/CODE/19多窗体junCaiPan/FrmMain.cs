using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace junCaiPan
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            //使用双缓冲,消除闪烁现象
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }

        private void FrmMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(CaiPanDClass.bmpViewNew, 0, 0, 600, 800); 
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            label1.Text = string.Format("本机IP地址：{0}",CaiPanDClass.localIP);
            label2.Text = string.Format("选手A：{0}", "");
            label3.Text = string.Format("选手B：{0}", "");
        }
        private void FrmMain_MouseMove(object sender, MouseEventArgs e)
        {
            CaiPanDClass.meReDrawMainWnd.Set();
            Invalidate();
        }
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case CaiPanDClass.WM_UPDATEINFOA:
                    label2.Text = CaiPanDClass.sOneTextInfo;
                    break;
                case CaiPanDClass.WM_UPDATEINFOB:
                    label3.Text = CaiPanDClass.sOneTextInfo;
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //开始对弈
            UdpCaipanTran.meCaiPanBeginWarUdpData.Set(); 
        } 
        private void FrmMain_MouseUp(object sender, MouseEventArgs e)
        {
            label4.Text = string.Format("X:{0};Y:{1}", e.X, e.Y);
        }
    }
}
