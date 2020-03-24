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

namespace pipe_c
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
NamedPipeClientStream pipeClient =new NamedPipeClientStream(".", "testpipe", PipeDirection.In);
pipeClient.Connect();
StreamReader sr = new StreamReader(pipeClient);
textBox1.Text=sr.ReadToEnd();

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
            else {
                //互斥量不存在
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
            m.ReleaseMutex();
            textBox1.AppendText("释放互斥量MutexExample\r\n");
            textBox1.ScrollToCaret();
        }
    }
}
