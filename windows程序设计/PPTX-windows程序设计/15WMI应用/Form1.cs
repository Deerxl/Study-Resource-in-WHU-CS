using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ex15
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        

        static private void HandleEvent(object sender, EventArrivedEventArgs e)
        {
            MessageBox.Show("U 盘插入");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
          
        }
        static void uWatch()
        {

            WqlEventQuery queryCreate = new WqlEventQuery("__InstanceCreationEvent",
            new TimeSpan(0, 0, 1),
           "TargetInstance ISA \"Win32_LogicalDisk\"");
            ManagementEventWatcher watcher =
            new ManagementEventWatcher(queryCreate);
            watcher.EventArrived += new EventArrivedEventHandler(HandleEvent);
            watcher.Start();
           while(!lin.WaitOne(500))
            {
                if(lin.WaitOne(500))
                {
                    watcher.Stop();
                }
            }
        }
        static ManualResetEvent uwatch_e;
        static ManualResetEvent lin;
        private void button1_Click(object sender, EventArgs e)
        {
            lin = new ManualResetEvent(false);
            uwatch_e = new ManualResetEvent(false);
            ThreadStart ts = new ThreadStart(uWatch);
            Thread th = new Thread(ts);
            th.IsBackground = true;
            th.Start();

        }
     private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
         lin.Set();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
