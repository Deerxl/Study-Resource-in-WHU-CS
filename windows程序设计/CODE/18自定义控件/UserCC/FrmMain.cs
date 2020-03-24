using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserCC
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

public void SwitchChange(bool t1)
{
    label1.Text=string.Format("RoundSwitch Sate is {0}",t1);
} 
private void FrmMain_Load(object sender, EventArgs e)
{
    roundSwitch1.SwitchOn = true;
    //向控件注册事件处理函数，用于切换要显示的窗体对象
    roundSwitch1.AddMouseClickEvent(SwitchChange);
}
    }
}
