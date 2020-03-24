using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FileMerge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string folder_path;
        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folder_path = folderBrowserDialog1.SelectedPath;
                label3.Text = folder_path;
            }
        }
        public static string[] folder_files;
        private void button6_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(folder_path))//检查文件目录是否存在
            {
                //搜索给定字符串的文件
                folder_files = Directory.GetFiles(folder_path,textBox1.Text,SearchOption.AllDirectories);
                listBox1.Items.Clear();
                int selected_index=0;
                foreach (string folder_file in folder_files)
                {
                    selected_index=listBox1.Items.Add(folder_file);
                    listBox1.SetSelected(selected_index, true);
                }                
            } 

        }

        private void button4_Click(object sender, EventArgs e)
        {

            foreach (String folder_file in listBox1.SelectedItems)
            {
                listBox2.Items.Add(folder_file);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int sel_index = listBox2.SelectedIndex;
            string sel_str=listBox2.SelectedItem.ToString();
            if(sel_index>0)
            {
                //将当前选中的项与前一项交换，并交换列表框的选中序号
                listBox2.Items[sel_index]=listBox2.Items[sel_index-1];
                listBox2.Items[sel_index-1]=sel_str;
                listBox2.SetSelected(sel_index,false);
                listBox2.SetSelected(sel_index-1, true);
            } 
        }
        public static string dest_file;
        private void button7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "选择要合并后的文件";
            saveFileDialog1.InitialDirectory = System.Environment.SpecialFolder.DesktopDirectory.ToString();
            saveFileDialog1.OverwritePrompt = false;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dest_file = saveFileDialog1.FileName;
                label2.Text = dest_file;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(File.Exists(dest_file))
            {
                //开始合并文件
                File.Delete(dest_file);
                FileStream fs_dest = new FileStream(dest_file, FileMode.CreateNew,FileAccess.Write);
                byte[] DataBuffer = new byte[100000];
                byte[] file_name_buf;
                //int file_name_len=0;
                FileStream fs_source=null;
                int read_len;

                FileInfo fi_a=null;
                for (int i = 0; i < listBox2.Items.Count;i++ )
                {
                    fi_a=new FileInfo(listBox2.Items[i].ToString());
                    file_name_buf=Encoding.Default.GetBytes(fi_a.Name);
                    //写入文件名
                    fs_dest.Write(file_name_buf, 0, file_name_buf.Length);
                    //换行
                    fs_dest.WriteByte((byte)13);
                    fs_dest.WriteByte((byte)10);
                    fs_source=new FileStream(fi_a.FullName,FileMode.Open,FileAccess.Read);
                    read_len=fs_source.Read(DataBuffer,0,100000);
                    while (read_len>0)
                    {                        
                        fs_dest.Write(DataBuffer, 0, read_len);
                        read_len = fs_source.Read(DataBuffer, 0, 100000);
                    }
                    //换行
                    fs_dest.WriteByte((byte)13);
                    fs_dest.WriteByte((byte)10);
                    fs_source.Close();
                }
                fs_source.Dispose();
                fs_dest.Flush();
                fs_dest.Close();
                fs_dest.Dispose();
                Process.Start(dest_file);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

            if (listBox2.SelectedItem != null)
            {
                Process.Start(listBox2.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("没有选中的文个件");
            }
            
            
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }
}
