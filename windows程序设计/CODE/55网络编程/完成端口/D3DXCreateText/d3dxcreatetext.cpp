//////////////////////////////////////////////////////////////////////////////////////////////////
// 
// File: d3dxcreatetext.cpp
// 
// Author: Frank Luna (C) All Rights Reserved
//
// System: AMD Athlon 1800+ XP, 512 DDR, Geforce 3, Windows XP, MSVC++ 7.0 
//
// Desc: Demonstrates how to create and render 3D Text using D3DXCreateText.
//          
//////////////////////////////////////////////////////////////////////////////////////////////////
#include "winsock2.h"
#include "d3dUtility.h"
#define DXNET_PORT 8133
//
// Globals
//
#define DATA_BUFSIZE 4096
WSABUF DataBuf;           // 定义WSABUF结构的缓冲区 
char byte_buffer[DATA_BUFSIZE];
DWORD dwBufferCount = 1, dwRecvBytes = 0, Flags = 0;
SOCKET ConnectSocket;

HANDLE h_sock_thread;

// #define WSAEVENT                HANDLE
//0 通知通信线程结束(主线程或者回调函数都行)；     1通信线程结束事件通知主线程
WSAEVENT ThreadQuitEventArray[2];  
WSAEVENT TXTFromClient_Event;//用于标识新字符串到达 
static TCHAR recvTXT[2000]; 
static int recvTxt_len;
// 建立需要的重叠结构，每个连入的SOCKET上的每一个重叠操作都得绑定一个   
WSAOVERLAPPED AcceptOverlapped ;// 如果要处理多个操作，这里当然需要一个   
// WSAOVERLAPPED数组   

IDirect3DDevice9* Device = 0; 

const int Width  = 1024;
const int Height = 800;

ID3DXMesh* Text = 0;

//
// Framework functions
//

HDC hdc;
LOGFONT lf;
HFONT hFont;
HFONT hFontOld;


void BuildAFont(LPCWSTR disTxt)
{
	//
	// Get a handle to a device context.
	//
	hdc = CreateCompatibleDC( 0 ); 

	//
	// Describe the font we want.
	//
 
	ZeroMemory(&lf, sizeof(LOGFONT));

	lf.lfHeight         = 25;    // in logical units
	lf.lfWidth          = 12;    // in logical units
	lf.lfEscapement     = 0;        
	lf.lfOrientation    = 0;     
	lf.lfWeight         = 500;   // boldness, range 0(light) - 1000(bold)
	lf.lfItalic         = false;   
	lf.lfUnderline      = false;    
	lf.lfStrikeOut      = false;    
	//lf.lfCharSet        = DEFAULT_CHARSET;
	lf.lfCharSet        = GB2312_CHARSET;
	lf.lfOutPrecision   = 0;              
	lf.lfClipPrecision  = 0;          
	lf.lfQuality        = 0;           
	lf.lfPitchAndFamily = 0;    
	//strcpy(lf.lfFaceName, "Times New Roman"); // font style
	wcscpy_s(lf.lfFaceName, TEXT("宋体")); // font style

	//
	// Create the font and select it with the device context.
	//
    hFont = CreateFontIndirect(&lf);
    hFontOld = (HFONT)SelectObject(hdc, hFont); 

	//
	// Create the text mesh based on the selected font in the HDC.
	//
    D3DXCreateText(Device, hdc,disTxt, 
        0.001f, 0.4f, &Text, 0, 0);

	//
	// Restore the old font and free the acquired HDC.
	//
    SelectObject(hdc, hFontOld);
    DeleteObject( hFont );
    DeleteDC( hdc );

}

bool Setup()
{
	//Socket 字节数组缓冲清空
	ZeroMemory(byte_buffer, DATA_BUFSIZE);
	DataBuf.len = DATA_BUFSIZE;
	DataBuf.buf = byte_buffer;



	BuildAFont(TEXT("欢迎大家来学习DirectX")); 
	//
	// Lights.
	//

	D3DXVECTOR3 dir(0.0f, -0.5f, 1.0f);
	D3DXCOLOR col = d3d::WHITE;
	D3DLIGHT9 light = d3d::InitDirectionalLight(&dir, &col);

	Device->SetLight(0, &light);
	Device->LightEnable(0, true);

	Device->SetRenderState(D3DRS_NORMALIZENORMALS, true);
	Device->SetRenderState(D3DRS_SPECULARENABLE, true);

	//
	// Set camera.
	//

	D3DXVECTOR3 pos(0.0f, 1.5f, -10.0f);
	D3DXVECTOR3 target(0.0f, 0.0f, 0.0f);
	D3DXVECTOR3 up(0.0f, 1.0f, 0.0f);

	D3DXMATRIX V;
	D3DXMatrixLookAtLH(
		&V,
		&pos,
		&target,
		&up);

	Device->SetTransform(D3DTS_VIEW, &V);

	//
	// Set projection matrix.
	//

	D3DXMATRIX proj;
	D3DXMatrixPerspectiveFovLH(
			&proj,
			D3DX_PI * 0.25f, // 45 - degree
			(float)Width / (float)Height,
			0.01f,
			1000.0f);
	Device->SetTransform(D3DTS_PROJECTION, &proj);

	return true;
}

void Cleanup()
{
	d3d::Release<ID3DXMesh*>(Text);
}



bool Display(float timeDelta)
{
	if( Device )
	{
		//
		// Update  the 3D text.
		// 
		if(WaitForSingleObject(TXTFromClient_Event,1)==WAIT_OBJECT_0)
		{//有网络数据已经到达，则重新更新字体
			BuildAFont(recvTXT);
			WSAResetEvent(TXTFromClient_Event);
		}


		D3DXMATRIX yRot, T;

		static float y = 0.0f;

		D3DXMatrixRotationY(&yRot, y);
		y += timeDelta*0.3;

		if( y >= 6.28f )
			y = 0.0f;

		D3DXMatrixTranslation(&T, -5.0f, 0.0f, 0.0f);
		T = T * yRot;

		Device->SetTransform(D3DTS_WORLD, &T);

		//
		// Render
		//

		Device->Clear(0, 0, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, 0x00000000, 1.0f, 0);
		Device->BeginScene();

		Device->SetMaterial(&d3d::WHITE_MTRL);
		Text->DrawSubset(0);

		Device->EndScene();
		Device->Present(0, 0, 0, 0);
	}
	return true;
} 

//
// WndProc
//
LRESULT CALLBACK d3d::WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch( msg )
	{
	case WM_DESTROY:
		::PostQuitMessage(0);
		break;
		
	case WM_KEYDOWN:
		if( wParam == VK_ESCAPE )
			::DestroyWindow(hwnd);
		break;
	}
	return ::DefWindowProc(hwnd, msg, wParam, lParam);
} 

void CALLBACK CompletionROUTINE(
								DWORD dwError, // 标志咱们投递的重叠操作，比如WSARecv，完成的状态是什么 
								DWORD cbTransferred, // 指明了在重叠操作期间，实际传输的字节量是多大 
								 LPWSAOVERLAPPED lpOverlapped, // 参数指明传递到最初的IO调用内的一个重叠  结构 
								DWORD dwFlags)  // 返回操作结束时可能用的标志(一般没用)
{//相应的完成例程函数 
		recvTxt_len=cbTransferred/2;
		MultiByteToWideChar(CP_ACP,0,DataBuf.buf,cbTransferred+1, recvTXT, 2000);
		//数据到达，通知主线程
		WSASetEvent(TXTFromClient_Event); 
		ZeroMemory(DataBuf.buf,DATA_BUFSIZE);
}
     

DWORD static FreshTXTThread( LPVOID lpParam ) 
{ //刷新获取网络字符数据
	int iResult;
	WORD wVersionRequested;
	WSADATA wsaData; 
	struct sockaddr_in clientService;   
	int err;
	 
	wVersionRequested = MAKEWORD( 2, 2 );
	err = WSAStartup( wVersionRequested, &wsaData );
	if ( err != 0 ) {
		/* Tell the user that we could not find a usable */
		/* WinSock DLL.                                  */
		return 1;
	}
	//创建远程服务器
	ConnectSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (ConnectSocket == INVALID_SOCKET) {
        //printf("socket failed with error: %ld\n", WSAGetLastError());
        WSACleanup();
        return 1;
    }

	//----------------------
    // The sockaddr_in structure specifies the address family,
    // IP address, and port of the server to be connected to.
    clientService.sin_family = AF_INET;
    //clientService.sin_addr.s_addr = inet_addr( "172.16.13.188" );
	clientService.sin_addr.s_addr = inet_addr( "127.0.0.1" );
    clientService.sin_port = htons(DXNET_PORT);

	// Connect to server.
    iResult = connect(ConnectSocket, (SOCKADDR*) &clientService, sizeof(clientService) );
    if (iResult == SOCKET_ERROR) {
        //printf( "connect failed with error: %d\n", WSAGetLastError() );
        closesocket(ConnectSocket);
        WSACleanup();
        return 1; 
	}
	bool bl_not_end_recv=false;
	char recvbuf[100]={0};
	 
	
	ZeroMemory(&AcceptOverlapped, sizeof(WSAOVERLAPPED));   

	//使用WSARecv来把我们的完成例程函数绑定上了   
	//CompletionRoutine函数已经定义好了 dwBufferCount is The number of WSABUF structures in the lpBuffers array
	//绑定一个Overlapped变量  
	

	//WSAWaitForMultipleEvents() API是一个比较奇怪的函数，它不仅具有WaitForMultipleObject函数的功能
	//更重要的是它还有一个功能，就是它能使线程处于等候完成例程事件，但这个完成例程的事件并不在它传入的事件数组里面
   //为了达到这个功能，需要在使用这个函数的时候将最后一个布尔变量值设为TRUE，这样线程就可以处于警觉
	//的等待事件，这个事件就是回调函数完成。   
	// #define WSAEVENT                HANDLE
	//WSAEVENT EventArray[1];        
	//EventArray[0] = WSACreateEvent();                        // 建立一个事件   
			////////////////////////////////////////////////////////////////////////////////   
	// 然后就等待重叠请求完成就可以了，注意保存返回值，这个很重要   
	//最后参数叫fAlertable ，can execute I/O completion routines
	//注意 fAlertable 参数一定要设置为 TRUE ,，线程就会置于一个警觉的等待状态
	DWORD dwIndex = 0;

	if(WSARecv(ConnectSocket, &DataBuf, dwBufferCount, &dwRecvBytes,    
	&Flags, &AcceptOverlapped, CompletionROUTINE)== SOCKET_ERROR)
	{//完成例程错误
		if(WSAGetLastError() != WSA_IO_PENDING)   
        {   //WSA_IO_PENDING ： 最常见的返回值，说明WSARecv操作成功了，但是I/O操作还没有完成
            closesocket(ConnectSocket);
			WSACleanup();
			return 1; 
        }    
	} 
	dwIndex = WSAWaitForMultipleEvents(1,ThreadQuitEventArray,FALSE,WSA_INFINITE,TRUE);  
	
	while(dwIndex!=WSA_WAIT_EVENT_0)//主线程通知通信线程结束事件
	{
		if(WSARecv(ConnectSocket, &DataBuf, dwBufferCount, &dwRecvBytes,    
		&Flags, &AcceptOverlapped, CompletionROUTINE)== SOCKET_ERROR)
		{//完成例程错误
			if(WSAGetLastError() != WSA_IO_PENDING)   
			{   //WSA_IO_PENDING ： 最常见的返回值，说明WSARecv操作成功了，但是I/O操作还没有完成
				closesocket(ConnectSocket);
				WSACleanup();
				WSASetEvent(ThreadQuitEventArray[1]);
				return 1; 
			}    
		} 
		dwIndex = WSAWaitForMultipleEvents(1,ThreadQuitEventArray,FALSE,WSA_INFINITE,TRUE);  
	} 
	  
	while(WSAWaitForMultipleEvents(1,ThreadQuitEventArray,FALSE,WSA_INFINITE,FALSE)!=(WSA_WAIT_EVENT_0));  
	closesocket(ConnectSocket);  
	WSACleanup( );  
	WSASetEvent(ThreadQuitEventArray[1]);
	return 0; 
}

//
// WinMain
//
int WINAPI WinMain(HINSTANCE hinstance,
				   HINSTANCE prevInstance, 
				   PSTR cmdLine,
				   int showCmd)
{
	if(!d3d::InitD3D(hinstance,
		Width, Height, true, D3DDEVTYPE_HAL, &Device))
	{
		::MessageBox(0, TEXT("InitD3D() - FAILED"), 0, 0);
		return 0;
	}
		
	if(!Setup())
	{
		::MessageBox(0, TEXT("Setup() - FAILED"), 0, 0);
		return 0;
	}

	TXTFromClient_Event=WSACreateEvent();
	WSAResetEvent(TXTFromClient_Event); 
	
	ThreadQuitEventArray[0] = WSACreateEvent();    //这个事件数组用于主线程通知通信线程结束   
	WSAResetEvent(ThreadQuitEventArray[0]);
	//创建通信线程--socket 客户端
	DWORD dwThreadId;
	h_sock_thread=CreateThread( 
            NULL,              // default security attributes
            0,                 // use default stack size  
            (LPTHREAD_START_ROUTINE)FreshTXTThread,          // thread function 
            NULL,             // argument to thread function 
            0,                 // use default creation flags 
            &dwThreadId);   // returns the thread identifier  

	d3d::EnterMsgLoop( Display );

	Cleanup(); 
	Device->Release(); 

	WSASetEvent(ThreadQuitEventArray[0]);

	//fAlertable 值这次应设为 FALSE;
	//反复调用，直到等待通信线程结束
	WSAWaitForMultipleEvents(1,ThreadQuitEventArray,TRUE,WSA_INFINITE,FALSE);
	return 0;
}

