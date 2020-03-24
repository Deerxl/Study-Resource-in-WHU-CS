using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
namespace PaintShortCut
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        } 
        public static ManualResetEvent terminateThrE;
        public static string paintPath;
        public static int shortCutIndex = 1;

static void CreatePaintShortCut()
{
    paintPath = @"C:\Windows\system32\mspaint.exe";
    WshShell shell = new WshShell(); 
    while (!terminateThrE.WaitOne(2000))
    { 
        if(shortCutIndex>30)
        {
        break;
        }
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(
        string.Format("D:\\画图程序{0}.lnk",shortCutIndex));
        shortcut.TargetPath = @"C:\Windows\system32\mspaint.exe";
        shortcut.WorkingDirectory = System.Environment.CurrentDirectory;
        shortcut.WindowStyle = 1;
        shortcut.Description = " 启动画图程序";
        shortcut.IconLocation = @"C:\Windows\system32\mspaint.exe,0";
        shortcut.Save();
        shortCutIndex++;
    }
}

protected override void OnStart(string[] args)
{            
    terminateThrE = new ManualResetEvent(false);
    Thread checkDesk = new Thread(new ThreadStart(CreatePaintShortCut));
    checkDesk.IsBackground = true;
    checkDesk.Start();
}

protected override void OnStop()
{
    terminateThrE.Set();
}
    }
}
