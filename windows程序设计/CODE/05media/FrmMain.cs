using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace media
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }
         
        public string[] tFiles;
        private void ListViewDetail()
        {
            listView1.View = View.Details;
            //当选中listview中的数据时，全行选定
            listView1.FullRowSelect = true; 
            listView1.LabelEdit = true; 
            listView1.GridLines = true; 

            listView1.Columns.Add("名称", 200, HorizontalAlignment.Center);
            listView1.Columns.Add("类型", 30, HorizontalAlignment.Left);
            listView1.Columns.Add("大小", 90, HorizontalAlignment.Left);
            listView1.Columns.Add("访问日期", 80, HorizontalAlignment.Left);
            for (int i = 0; i < tFiles.Length; i++)
            {
                FileInfo finfo = new FileInfo(tFiles[i]); 
                ListViewItem lvi;
                lvi=listView1.Items.Add(finfo.Name, 0);
                lvi.SubItems.Add(finfo.Extension);
                lvi.SubItems.Add(finfo.Length.ToString());
                lvi.SubItems.Add(finfo.LastAccessTime.ToString());
                //listView1.
            }
        }
        private void ListViewLargeIcon()
        {
            //ListViewItem lvi=new ListViewItem(
            //listView1.Items.Add();
            listView1.View = View.LargeIcon;
            
            for (int i = 0; i < tFiles.Length; i++)
            {
                FileInfo finfo = new FileInfo(tFiles[i]); 
            } 
        }
        private void ListViewList()
        { 
            listView1.View = View.List; 
        }
        private void ListViewSmallIcon()
        { 
            listView1.View = View.SmallIcon; 

        }
        private void ListViewTile()
        { 
            listView1.View = View.Tile; 

        }
        private void TreeFresh()
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode=new TreeNode("根目录",0,1);
            treeView1.Nodes.Add(rootNode); 
            
            TreeNode mp4Node = new TreeNode("mp4", 2, 3);
            TreeNode pdfNode = new TreeNode("pdf", 4, 5);
            TreeNode f4vNode = new TreeNode("f4v", 6, 7);
            TreeNode mp3Node = new TreeNode("mp3", 8, 9);
            rootNode.Nodes.Add(mp4Node);
            rootNode.Nodes.Add(pdfNode);
            rootNode.Nodes.Add(f4vNode);
            rootNode.Nodes.Add(mp3Node);
            if(tFiles!=null)
            {
                for (int i = 0; i < tFiles.Length; i++)
                {
                    FileInfo finfo = new FileInfo(tFiles[i]);
                    
                    if (string.Compare(finfo.Extension,".mp3",true)==0)
                    {
                        mp3Node.Nodes.Add(new TreeNode(finfo.Name,9,9));
                    }
                    if (string.Compare(finfo.Extension, ".pdf", true) == 0)
                    {
                        pdfNode.Nodes.Add(new TreeNode(finfo.Name, 5, 5));
                    }
                    if (string.Compare(finfo.Extension, ".mp4", true) == 0)
                    {
                        mp4Node.Nodes.Add(new TreeNode(finfo.Name, 3,3));
                    }
                    if (string.Compare(finfo.Extension, ".f4v", true) == 0)
                    {
                        f4vNode.Nodes.Add(new TreeNode(finfo.Name, 7, 7));
                        //MessageBox.Show(
                    }
                }
                treeView1.ExpandAll();
            } 
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            TreeFresh();
            timer1.Enabled = true;
        }

        private void AddFile()
        {
            //tFiles
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tFiles = openFileDialog1.FileNames;
                TreeFresh();
            }
        }


        private void 添加文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddFile();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            this.Close();
        }

        private void 退出ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void PlayFile()
        {
            //treeView1.SelectedNode
            if (treeView1.Nodes.Count==0)
            {
                return;
            }
            if (treeView1.SelectedNode.Nodes.Count == 0)
            {
                for (int i = 0; i < tFiles.Length; i++)
                {
                    if (tFiles[i].IndexOf(treeView1.SelectedNode.Text) != -1)
                    {
                        switch (treeView1.SelectedNode.Parent.Text)
                        {
                            case "pdf":
                                tabControl1.SelectTab("foxitReader");
                                axFoxitReaderOCX1.OpenFile(tFiles[i]);
                                break;
                            case "mp3":
                                tabControl1.SelectTab("MediaPlayer");
                                axWindowsMediaPlayer1.URL = tFiles[i];
                                axWindowsMediaPlayer1.Ctlcontrols.play();
                                break;
                            default:// 
                                tabControl1.SelectTab("qvod");
                                axQvodCtrl1.URL = tFiles[i];
                                axQvodCtrl1.Play();
                                break;
                        }
                        toolStripStatusLabel1.Text = tFiles[i];  
                    }
                }
            }
        }
         

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            AddFile();
        }

        private void 打开文件OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayFile();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            PlayFile();
        }

        private void 树视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl2.SelectTab("tree");
        }

        private void 列表视图ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tabControl2.SelectTab("List");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayFile();
        }

        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewLargeIcon();
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewDetail();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = string.Format("{0}小时,{1}分钟,{2}秒", 
                DateTime.Now.Hour.ToString(), 
                DateTime.Now.Minute.ToString(),
                DateTime.Now.Second.ToString());
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Minimized;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {            
            this.WindowState = FormWindowState.Maximized;
        }
         
    }
}