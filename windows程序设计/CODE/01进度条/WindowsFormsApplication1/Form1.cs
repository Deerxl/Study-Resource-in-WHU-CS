using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                //用来读入文本的类
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                textBox1.Text = sr.ReadToEnd();
                //关闭文件
                sr.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(progressBar1.Value>=progressBar1.Maximum)
            {
                //判断进度条的进度值是否已经完成
                timer1.Enabled = false;
            }else
            {
                progressBar1.Value += 1;
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }
    }
}
