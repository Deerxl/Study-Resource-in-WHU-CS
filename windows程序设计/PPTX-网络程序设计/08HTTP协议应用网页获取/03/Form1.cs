using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;

namespace GetHtm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

// State object for receiving data from remote device.
public class StateObject
{
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 256;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

// The port number for the remote device.
private const int port = 80;
// ManualResetEvent instances signal completion.
private static ManualResetEvent connectDone = new ManualResetEvent(false);
private static ManualResetEvent sendDone = new ManualResetEvent(false);
private static ManualResetEvent receiveDone = new ManualResetEvent(false);

// The response from the remote device.
private static String request = String.Empty;
private static String response = String.Empty; 
public static string url_str;
public const int UPDATE_SEND = 0x500;
public const int UPDATE_RECEIVE = 0x501; 
public static IntPtr main_wnd_handle;


public static MemoryStream ms_recv;
//动态链接库引入
[DllImport("User32.dll", EntryPoint = "SendMessage")]
private static extern int SendMessage(
IntPtr hWnd, // handle to destination window 
int Msg, // message 
int wParam, // first message parameter 
int lParam // second message parameter 
);


public static string httpfile = "httpd.rar";

protected override void DefWndProc(ref Message m)
{//窗体消息处理重载
    switch (m.Msg)
    {
        case UPDATE_SEND:
            //对发送的字节信息进行显示 
            textBox1.AppendText(request+"\r\n");
            textBox1.ScrollToCaret();
            break;
        case UPDATE_RECEIVE:
            //对接收的字节信息进行显示
            textBox3.AppendText(response);
            textBox3.ScrollToCaret();
            break;
        default:
            base.DefWndProc(ref m);
            break;
    }
}
static void MultiThreadsDown(Object tdata)
{
    try
    {
        // Establish the remote endpoint for the socket. 
        IPAddress ipAddress = Dns.GetHostEntry(url_str).AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        // Create a TCP/IP socket.
        Socket client = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        // Connect to the remote endpoint.
        client.Connect(remoteEP);

        string httprequest;
        httprequest = "GET /" + httpfile + " HTTP/1.1\r\n";
        httprequest += "Connection: close\r\n";
        httprequest += "Range: bytes=0-\r\n";
        httprequest += "Host: 192.168.1.100\r\n";
        httprequest += "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)\r\n\r\n";

        ThreadPara thdata = (ThreadPara)tdata;

        thdata.msFData.Seek(0, SeekOrigin.Begin);
        byte[] sendData = Encoding.ASCII.GetBytes(httprequest);
        client.Send(sendData, SocketFlags.None);
        byte[] receiveData = new byte[1024];
        Int32 recvLen = client.Receive(receiveData, 1024, SocketFlags.None);
        thdata.msFData.Write(receiveData, 0, recvLen);
        while (recvLen > 0)
        {
            recvLen = client.Receive(receiveData, 1024, SocketFlags.None);
            thdata.msFData.Write(receiveData, 0, recvLen);
        } 
        client.Shutdown(SocketShutdown.Both);
        client.Close();


        thdata.eventThreadDone.Set();
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }


}




static void thread_GET_html()
{ 
    try
    {
        // Establish the remote endpoint for the socket. 
        IPAddress ipAddress =  Dns.GetHostEntry(url_str).AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port); 
        // Create a TCP/IP socket.
        Socket client = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp); 
        // Connect to the remote endpoint.
        client.BeginConnect(remoteEP,
            new AsyncCallback(ConnectCallback), client);
        connectDone.WaitOne(); 
        // Send test data to the remote device.
        request = "HEAD /" + httpfile + " HTTP/1.1\r\n"; 
        request += "Connection: close\r\n";
        request += "Range: bytes=0-\r\n";
        request += "Host: 192.168.1.100\r\n"; 
        request += "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)\r\n\r\n";

        Send(client, request);
        SendMessage(main_wnd_handle, UPDATE_SEND, 100, 100);
        sendDone.WaitOne(); 
        // Receive the response from the remote device.
        Receive(client);
        receiveDone.WaitOne();  
        // Release the socket.
        client.Shutdown(SocketShutdown.Both);
        client.Close();

        //获取文件长度信息
        long dFileLen = 0;
        string strFilelen;
        string[] responLines = totalrespon.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < responLines.Length;i++ )
        {
            if (responLines[i].IndexOf("Content-Range")>-1)
            { 
                strFilelen = responLines[i].Substring(responLines[i].IndexOf("/") + 1);
                dFileLen=Int64.Parse(strFilelen);
                break;
            }
        }


        msFileData0 = new MemoryStream(5000000);
        msFileData1 = new MemoryStream(5000000);

        threadsDone[0] = new ManualResetEvent(false);
        threadsDone[1] = new ManualResetEvent(false);

        //初始化线程参数
        ThreadPara thrPara0 = new ThreadPara();
        thrPara0.msFData = msFileData0;
        thrPara0.eventThreadDone = threadsDone[0];
        thrPara0.dataLen = dFileLen;


        ThreadPara thrPara1 = new ThreadPara();
        thrPara1.msFData = msFileData1;
        thrPara1.eventThreadDone = threadsDone[1];  

        //启动线程下载文件不同部分 
        Thread Thread0 = new Thread(new ParameterizedThreadStart(MultiThreadsDown));
        Thread0.Start(thrPara0);

        thrPara1.eventThreadDone.Set();
        WaitHandle.WaitAll(threadsDone);

         //合并文件内容
        byte[] temBuf=new byte[1024];
        string filename = "d:\\" + httpfile ;
        FileStream fs = new FileStream(filename, FileMode.CreateNew);
        long totalLen=0;
        int writeLen=0;
        thrPara0.msFData.Seek(0,SeekOrigin.Begin);

        byte[] tData=new byte[4];
        int j = 0;
        for (; j < 1000;j++ )
        {
            thrPara0.msFData.Seek(j,SeekOrigin.Begin);
            thrPara0.msFData.Read(tData,0,4);
            if ((tData[0]==13)&&(tData[1]==10)&&(tData[2]==13)&&(tData[3]==10))
            {
                thrPara0.msFData.Seek(j+4, SeekOrigin.Begin);
                break;
            }
        } 

        do{  
            writeLen=thrPara0.msFData.Read(temBuf,0,1024);
            fs.Write(temBuf,0,writeLen);
            totalLen+=writeLen;            
        }
        while(totalLen<thrPara0.dataLen);
        
        
        fs.Flush();
        fs.Close();
        fs.Dispose();
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}

static ManualResetEvent[] threadsDone = new ManualResetEvent[2];


public static MemoryStream msFileData0;
public static MemoryStream msFileData1;
public class ThreadPara
{
    public long StartPos;
    public long EndPos;
    public ManualResetEvent eventThreadDone;
    public MemoryStream msFData;
    public long dataLen;
}





private static void ConnectCallback(IAsyncResult ar)
{
    try
    {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState; 
        // Complete the connection.
        client.EndConnect(ar); 
        request = "连接成功";
        SendMessage(main_wnd_handle, UPDATE_SEND, 100, 100);
        connectDone.Set();
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}



private static void Receive(Socket client)
{
    try
    {
        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = client;

        // Begin receiving the data from the remote device.
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReceiveCallback), state);
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}

public static string totalrespon;
private static void ReceiveCallback(IAsyncResult ar)
{
    try
    {
        // Retrieve the state object and the client socket 
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;

        // Read data from the remote device.
        int bytesRead = client.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There might be more data, so store the data received so far.
            //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));  
            //Encoding gb2312 = Encoding.GetEncoding("gb2312");
            //response = gb2312.GetString(state.buffer, 0, bytesRead);
            response = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

            totalrespon += response;
            SendMessage(main_wnd_handle, UPDATE_RECEIVE, 100, 100);
            // Get the rest of the data.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        else
        {
            // All the data has arrived; put it in response.
            if (state.sb.Length > 1)
            {
                response = state.sb.ToString();
            }
            // Signal that all bytes have been received.
            receiveDone.Set();
        }
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}

private static void Send(Socket client, String data)
{
    // Convert the string data to byte data using ASCII encoding.
    byte[] byteData = Encoding.ASCII.GetBytes(data); 
    // Begin sending the data to the remote device.
    client.BeginSend(byteData, 0, byteData.Length, 0,
        new AsyncCallback(SendCallback), client);
}

private static void SendCallback(IAsyncResult ar)
{
    try
    {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        int bytesSent = client.EndSend(ar);
        request = String.Format("Sent {0} bytes to server.", bytesSent); 
        SendMessage(main_wnd_handle, UPDATE_SEND, 100, 100);

        // Signal that all bytes have been sent.
        sendDone.Set();
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}

        private void button1_Click(object sender, EventArgs e)
        {
            url_str = textBox2.Text;
            //启动线程			
            Thread workThread = new Thread(new ThreadStart(thread_GET_html));
            workThread.IsBackground = true;
            workThread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            main_wnd_handle = this.Handle;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

    }
}
