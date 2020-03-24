using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Security.AccessControl;

namespace pipe_s
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.Out);
            textBox1.Text = "等待客户连接";
            pipeServer.WaitForConnection();
            try
            {
                StreamWriter sw = new StreamWriter(pipeServer);
                sw.AutoFlush = true;
                sw.WriteLine("通过管道来到的信息");
                sw.Close(); 
            }
            catch (IOException e1)
            {
                MessageBox.Show("ERROR: {0}", e1.Message);
            }
        }

        public Mutex m;

        private void button2_Click(object sender, EventArgs e)
        {
            bool createNew;
            m = new Mutex(false, "MutexExample", out createNew);
            if (createNew)
            {
                //互斥量不存在，创建新实例 
                textBox1.AppendText("创建互斥量MutexExample\r\n");
                textBox1.ScrollToCaret();
            }
            else
            {
                //互斥量已存在
                textBox1.AppendText("互斥量MutexExample已存在\r\n");
                textBox1.ScrollToCaret();
            } 
            //互斥量存在，获取拥有
            m.WaitOne();
            textBox1.AppendText("拥有互斥量MutexExample\r\n");
            textBox1.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
                bool createNew;
                m = new Mutex(false, "MutexExample", out createNew);

                try
                {
                    m.ReleaseMutex();
                    textBox1.AppendText("释放互斥量MutexExample\r\n");
                    textBox1.ScrollToCaret();
                }
                catch (ApplicationException)
                {
                    textBox1.AppendText("当前线程不拥有互斥量MutexExample，无法执行释放操作\r\n");
                    textBox1.ScrollToCaret();
                }
            
        }
    }
}
