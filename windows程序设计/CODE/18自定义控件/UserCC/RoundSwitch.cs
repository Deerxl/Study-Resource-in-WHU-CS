using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UserCC.Properties;

namespace UserCC
{
    public partial class RoundSwitch : UserControl
    {
public RoundSwitch()
{
    InitializeComponent();
    //减少闪烁
    this.SetStyle(ControlStyles.UserPaint, true);
    this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    this.SetStyle(ControlStyles.DoubleBuffer, true);
}

//开关状态变量
public bool mSwitchOn;
//设置属性值在属性窗口的可见性
[Browsable(true)]
public bool SwitchOn
{
    get
    {
        return mSwitchOn;
    }
    set
    {
        mSwitchOn = value;
        Invalidate();
    }
}


//自定义绘制控件
private void DrawCtl(PaintEventArgs e)
{
    //绘制图片内容
    Bitmap theBmp1;
    Bitmap theBmp2;
    Rectangle destRect = new Rectangle(0, 0, 200, 200);
    //绘制头图片 
    theBmp1 = Resources.butn01;
    theBmp2 = Resources.butn02;
    if (mSwitchOn)
    {
        e.Graphics.DrawImage(theBmp1, destRect);
    }
    else
    {
        e.Graphics.DrawImage(theBmp2, destRect);
    }
}
        //重载绘图函数，调用自定义绘制函数
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaint(e);
    DrawCtl(e);
}

        private void RoundSwitch_Load(object sender, EventArgs e)
        {
            mSwitchOn = false;
        }

public delegate void ControlDelegate(bool t1);
public event ControlDelegate ProcessEvent;
public void AddMouseClickEvent(ControlDelegate cde)
{
    ProcessEvent += cde;
}
 

private void RoundSwitch_MouseClick(object sender, MouseEventArgs e)
{
    //控件响应用户鼠示点击，修改自身变量值
    mSwitchOn = !mSwitchOn; 
    //调用控件的"事件"回调函数
    ProcessEvent(mSwitchOn);
    Invalidate();
}


    }
}
