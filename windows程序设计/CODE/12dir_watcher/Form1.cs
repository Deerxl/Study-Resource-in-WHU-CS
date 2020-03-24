using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace dir_watcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string w_dir;
        private void button1_Click(object sender, EventArgs e)
        {
            e_end_thread = new ManualResetEvent(false);
            if(folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                w_dir = folderBrowserDialog1.SelectedPath; 
            }
        }

        public static ManualResetEvent e_end_thread; 
         
        public static void moveFile()
        { 
        
            long now_t=DateTime.Now.ToFileTime();

            DirectoryInfo d_info = new DirectoryInfo(w_dir);
            string new_filename;
            FileInfo[] f_ins=d_info.GetFiles();

            while (!e_end_thread.WaitOne(500))
            {
                for (int i = 0; i < f_ins.Length; i++)
                {
                    now_t = DateTime.Now.ToFileTime();
                    if (File.Exists(f_ins[i].FullName))
                    {
                        new_filename = w_dir + "\\ref\\" + now_t.ToString() + f_ins[i].Name;
                        if (!File.Exists(new_filename))
                        {
                            File.Move(f_ins[i].FullName, new_filename);
                        } 
                    } 
                }
                f_ins = d_info.GetFiles();//重新获取新的目录信息
            } 
        } 

        private void button2_Click(object sender, EventArgs e)
        {
            e_end_thread.Reset(); 
            Thread workThread = new Thread(new ThreadStart(moveFile));
            workThread.IsBackground = true;
            workThread.Start(); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            e_end_thread.Set();
        }


    }
}
