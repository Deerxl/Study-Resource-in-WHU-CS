using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;
using System.Runtime.InteropServices;
using System.Threading;

namespace capwol
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("User32.dll")]
        private static extern int SendMessage(
            IntPtr hWnd,
            int Msg,
            int  wParam,
            int lParam
            );
        public const int UPDATE_CAP=0x500;
        public static IntPtr mainWndHandle;
        protected override void DefWndProc(ref Message m)
        {
            switch(m.Msg)
            {
                case UPDATE_CAP:
                    textBox1.AppendText(strCap+"\r\n");
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
            
        }
        private static string  strCap;
        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var time = e.Packet.Timeval.Date;
            var len = e.Packet.Data.Length;
            strCap=string.Format("{0}:{1}:{2},{3} Len={4}",
                time.Hour, time.Minute, time.Second, time.Millisecond, len);
            SendMessage(mainWndHandle, UPDATE_CAP, 100, 100);
            // parse the incoming packet
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if (packet == null)
                return;

            var wol = PacketDotNet.WakeOnLanPacket.GetEncapsulated(packet);
            //if (wol.PayloadData != null)
            if (wol != null)
            {
                byte[]macAddB = wol.DestinationMAC.GetAddressBytes();
                strCap = string.Format("{0:X2}-{1:X2}-{2:X2}-{3:X2}-{4:X2}-{5:X2}", macAddB[0], macAddB[1], macAddB[2], macAddB[3], macAddB[4], macAddB[5]);    
                SendMessage(mainWndHandle, UPDATE_CAP, 100, 100); 
            }
        }


        public static ICaptureDevice device;
        private static void capData()
        {
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(device_OnPacketArrival);
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            device.Filter = "ether dst FF:FF:FF:FF:FF:FF and udp";
            // start capture packets
            device.Capture();
            device.Close();
        }
         
        private void button1_Click(object sender, EventArgs e)
        {
            string ver = SharpPcap.Version.VersionString;
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                textBox1.AppendText("没有发现网卡\r\n");
                textBox1.ScrollToCaret();
                return;
            }
            foreach (var dev in devices)
            {
                textBox1.AppendText(string.Format("{0}\r\n",dev.Description));
            }

            device = devices[0];

            ThreadStart theSta = new ThreadStart(capData);
            Thread thr = new Thread(theSta);
            thr.Start();


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainWndHandle = this.Handle;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            device.StopCapture();
        }


    }
}
