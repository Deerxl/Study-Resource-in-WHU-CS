using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Threading;
namespace wmi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        { 
SelectQuery query = new SelectQuery("Select * From Win32_LogicalDisk");
ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
textBox1.Text = "";

//1 No type
//2 Floppy disk
//3 Hard disk
//4 Removable drive or network drive
//5 CD-ROM
//6 RAM disk 
foreach (ManagementBaseObject disk in searcher.Get())
{
    textBox1.Text+=disk["Name"] + " " + disk["DriveType"] + " " + disk["VolumeName"]+"\r\n";
}
             
            //SelectQuery selectQuery = new
            //SelectQuery("Win32_LogicalDisk");
            //ManagementObjectSearcher searcher =
            //    new ManagementObjectSearcher(selectQuery);

            //foreach (ManagementObject disk in searcher.Get())
            //{
            //    Console.WriteLine(disk.ToString());
            //}


        }

        private void button2_Click(object sender, EventArgs e)
        {
            String HDid;
            ManagementClass cimobject = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc = cimobject.GetInstances();
            textBox1.Text = "";
            foreach (ManagementObject mo in moc)
            {
                HDid = (string)mo.Properties["Model"].Value;
                textBox1.Text += HDid + "\r\n";
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            string cpuInfo = "";//cpu序列号
            ManagementClass cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            textBox1.Text = "";
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                textBox1.Text += cpuInfo + "\r\n";  
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
ManagementObjectCollection moc = mc.GetInstances();
textBox1.Text = "";
foreach(ManagementObject mo in moc)
{
    if((bool)mo["IPEnabled"] == true) 
    textBox1.Text += string.Format("MAC address   {0}", mo["MacAddress"].ToString()) + "\r\n";
    mo.Dispose();
}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard")
            SelectQuery query = new SelectQuery("SELECT * FROM Win32_BaseBoard");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementBaseObject board in searcher.Get())
            {
                textBox1.Text += "制造商:"+board["Manufacturer"] + " 型号：" + board["Product"] + " 序列号：" + board["SerialNumber"] + "\r\n";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ManagementClass mClass = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection moCollection = mClass.GetInstances();
            textBox1.Text = "";
            foreach (ManagementObject mObject in moCollection)
            {
                textBox1.Text += mObject["SerialNumber"].ToString();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
ManagementObjectCollection moCollection = searcher.Get();
textBox1.Text = "";
foreach (ManagementObject mObject in moCollection)
{
    textBox1.Text += mObject["SerialNumber"].ToString() + " ";
}

        }

        private void button8_Click(object sender, EventArgs e)
        {
            SelectQuery query =
            new SelectQuery("Win32_Environment"); 
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(query);
            textBox1.Text = "";
            foreach (ManagementObject envVar in searcher.Get())
                 textBox1.Text +=string.Format("Variable : {0}, Value = {1}",
                envVar["Name"], envVar["VariableValue"])+"\r\n";

        }

        private void button9_Click(object sender, EventArgs e)
        {
            ManagementClass osClass = new ManagementClass("Win32_OperatingSystem");
            ulong PhysicalMemorySize, VirtualMemorySize, FreePhysicalMemory;
            PhysicalMemorySize = 0;
            VirtualMemorySize = 0;
            FreePhysicalMemory = 0;
            textBox1.Text = "";
            foreach (ManagementObject obj in osClass.GetInstances())
            {
                if (obj["TotalVisibleMemorySize"] != null)
                     PhysicalMemorySize = (ulong)obj["TotalVisibleMemorySize"];

                if (obj["TotalVirtualMemorySize"] != null)
                     VirtualMemorySize = (ulong)obj["TotalVirtualMemorySize"];

                if (obj["FreePhysicalMemory"] != null)
                     FreePhysicalMemory = (ulong)obj["FreePhysicalMemory"];
                break;
             }
            textBox1.Text = string.Format("物理内存数为{0};\r\n虚内存数为{1};\r\n可用内存数为{2}", 
                PhysicalMemorySize, VirtualMemorySize, FreePhysicalMemory);

            
        }

        private void button10_Click(object sender, EventArgs e)
        {
ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
ManagementObjectCollection moCollection = searcher.Get();
 
textBox1.Text = ""; 
foreach (ManagementObject mo in moCollection) 
{ 
    string state;
    if (mo["Started"].Equals(true))
        state = "Started"; 
    else
        state = "Stop";
    textBox1.AppendText(string.Format("服务名：{0}，启动方式：{1}，状态：{2}，启动账号：{3}\r\n",
        mo["Name"].ToString(), mo["StartMode"].ToString(), state, mo["StartName"].ToString())); 
}   
        }

static void uWatch()
{

//TargetInstance.DriveType = 2 
//代表判断Win32_LogicalDisk.DriveType属性，2则代表可移动磁盘  
WqlEventQuery queryCreate = new WqlEventQuery("__InstanceCreationEvent",
    new TimeSpan(0, 0, 1),
    //    "TargetInstance isa \"Win32_Process\"");

"TargetInstance ISA \"Win32_LogicalDisk\"");
// AND TargetInstance.DriveType = 2
ManagementEventWatcher watcher =
    new ManagementEventWatcher(queryCreate);
//watcher.Options.Timeout = new TimeSpan(0, 0, 35);

ManagementBaseObject ex = watcher.WaitForNextEvent();
//textBox1.Text = ((ManagementBaseObject)ex["TargetInstance"])["Name"].ToString();
MessageBox.Show("U盘插入");
watcher.EventArrived += new EventArrivedEventHandler(HandleEvent);
watcher.Start();

uwatch_e.WaitOne(60000);

watcher.Stop();
}
static ManualResetEvent uwatch_e;
private void button11_Click(object sender, EventArgs e)
{
    uwatch_e = new ManualResetEvent(false);

    ThreadStart ts = new ThreadStart(uWatch);
    Thread th = new Thread(ts);
    th.IsBackground = true;
    th.Start();
}
static private void HandleEvent(object sender,EventArrivedEventArgs e)
{ 
    MessageBox.Show("U盘插入");
    uwatch_e.Set();
}

        private void button12_Click(object sender, EventArgs e)
        {
            uwatch_e.Set();
        }


    }
}
