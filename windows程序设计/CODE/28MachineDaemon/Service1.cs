﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace MachineDaemon
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            CanPauseAndContinue = true;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]

        internal static extern bool ExitWindowsEx(int DoFlag, int rea);
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;


        private static void DoExitWin(int DoFlag)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(DoFlag, 0);
        }



        public static int bootMinute = 0;
        public static ManualResetEvent e_1;
        public static ManualResetEvent e_2;
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("MachineDaemon OnStart", DateTime.Now.ToLongTimeString());   
            bootMinute = 3;
            e_1 = new ManualResetEvent(false);
            e_2 = new ManualResetEvent(false);
            e_1.Reset();
            e_2.Reset();
            do
            {//首先等待机器三分钟
                if ((Environment.TickCount / 180000) > bootMinute)
                {
                    e_1.Set();
                }
                e_1.WaitOne(20000,false);
            } while (e_1.WaitOne(1,false));
            EventLog.WriteEntry("MachineDaemon check three minutes", DateTime.Now.ToLongTimeString());
            DateTime cur_time;
            do
            { //每分钟检查一次
                cur_time = DateTime.Now;
                if ((cur_time.Hour < 7) || (cur_time.Hour > 21))
                {//当前时间介于22点至6点，则关机
                    e_2.Set();                    
                }
                e_2.WaitOne(30000,false);
            } while (!e_2.WaitOne(1,false));

            EventLog.WriteEntry("MachineDaemon DoExitWin", DateTime.Now.ToLongTimeString());

            DoExitWin(EWX_FORCE | EWX_POWEROFF); 

        }

        protected override void OnStop()
        {
        }
        protected override void  OnPause()
        {
             EventLog.WriteEntry("MachineDaemon OnPause", DateTime.Now.ToLongTimeString());   
 	         base.OnPause();
        } 
        protected override void OnContinue()
        {
            EventLog.WriteEntry("MachineDaemon OnContinue", DateTime.Now.ToLongTimeString());
            base.OnContinue();
        }
        protected override void OnCustomCommand(int command)
        {
            
            base.OnCustomCommand(command);
        }
    }
}
