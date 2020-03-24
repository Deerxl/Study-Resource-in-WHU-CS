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
WSABUF DataBuf;           // ����WSABUF�ṹ�Ļ����� 
char byte_buffer[DATA_BUFSIZE];
DWORD dwBufferCount = 1, dwRecvBytes = 0, Flags = 0;
SOCKET ConnectSocket;

HANDLE h_sock_thread;

// #define WSAEVENT                HANDLE
//0 ֪ͨͨ���߳̽���(���̻߳��߻ص���������)��     1ͨ���߳̽����¼�֪ͨ���߳�
WSAEVENT ThreadQuitEventArray[2];  
WSAEVENT TXTFromClient_Event;//���ڱ�ʶ���ַ������� 
static TCHAR recvTXT[2000]; 
static int recvTxt_len;
// ������Ҫ���ص��ṹ��ÿ�������SOCKET�ϵ�ÿһ���ص��������ð�һ��   
WSAOVERLAPPED AcceptOverlapped ;// ���Ҫ���������������ﵱȻ��Ҫһ��   
// WSAOVERLAPPED����   

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
	wcscpy_s(lf.lfFaceName, TEXT("����")); // font style

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
	//Socket �ֽ����黺�����
	ZeroMemory(byte_buffer, DATA_BUFSIZE);
	DataBuf.len = DATA_BUFSIZE;
	DataBuf.buf = byte_buffer;



	BuildAFont(TEXT("��ӭ�����ѧϰDirectX")); 
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
		{//�����������Ѿ���������¸�������
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
								DWORD dwError, // ��־����Ͷ�ݵ��ص�����������WSARecv����ɵ�״̬��ʲô 
								DWORD cbTransferred, // ָ�������ص������ڼ䣬ʵ�ʴ�����ֽ����Ƕ�� 
								 LPWSAOVERLAPPED lpOverlapped, // ����ָ�����ݵ������IO�����ڵ�һ���ص�  �ṹ 
								DWORD dwFlags)  // ���ز�������ʱ�����õı�־(һ��û��)
{//��Ӧ��������̺��� 
		recvTxt_len=cbTransferred/2;
		MultiByteToWideChar(CP_ACP,0,DataBuf.buf,cbTransferred+1, recvTXT, 2000);
		//���ݵ��֪ͨ���߳�
		WSASetEvent(TXTFromClient_Event); 
		ZeroMemory(DataBuf.buf,DATA_BUFSIZE);
}
     

DWORD static FreshTXTThread( LPVOID lpParam ) 
{ //ˢ�»�ȡ�����ַ�����
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
	//����Զ�̷�����
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

	//ʹ��WSARecv�������ǵ�������̺���������   
	//CompletionRoutine�����Ѿ�������� dwBufferCount is The number of WSABUF structures in the lpBuffers array
	//��һ��Overlapped����  
	

	//WSAWaitForMultipleEvents() API��һ���Ƚ���ֵĺ���������������WaitForMultipleObject�����Ĺ���
	//����Ҫ����������һ�����ܣ���������ʹ�̴߳��ڵȺ���������¼��������������̵��¼���������������¼���������
   //Ϊ�˴ﵽ������ܣ���Ҫ��ʹ�����������ʱ�����һ����������ֵ��ΪTRUE�������߳̾Ϳ��Դ��ھ���
	//�ĵȴ��¼�������¼����ǻص�������ɡ�   
	// #define WSAEVENT                HANDLE
	//WSAEVENT EventArray[1];        
	//EventArray[0] = WSACreateEvent();                        // ����һ���¼�   
			////////////////////////////////////////////////////////////////////////////////   
	// Ȼ��͵ȴ��ص�������ɾͿ����ˣ�ע�Ᵽ�淵��ֵ���������Ҫ   
	//��������fAlertable ��can execute I/O completion routines
	//ע�� fAlertable ����һ��Ҫ����Ϊ TRUE ,���߳̾ͻ�����һ�������ĵȴ�״̬
	DWORD dwIndex = 0;

	if(WSARecv(ConnectSocket, &DataBuf, dwBufferCount, &dwRecvBytes,    
	&Flags, &AcceptOverlapped, CompletionROUTINE)== SOCKET_ERROR)
	{//������̴���
		if(WSAGetLastError() != WSA_IO_PENDING)   
        {   //WSA_IO_PENDING �� ����ķ���ֵ��˵��WSARecv�����ɹ��ˣ�����I/O������û�����
            closesocket(ConnectSocket);
			WSACleanup();
			return 1; 
        }    
	} 
	dwIndex = WSAWaitForMultipleEvents(1,ThreadQuitEventArray,FALSE,WSA_INFINITE,TRUE);  
	
	while(dwIndex!=WSA_WAIT_EVENT_0)//���߳�֪ͨͨ���߳̽����¼�
	{
		if(WSARecv(ConnectSocket, &DataBuf, dwBufferCount, &dwRecvBytes,    
		&Flags, &AcceptOverlapped, CompletionROUTINE)== SOCKET_ERROR)
		{//������̴���
			if(WSAGetLastError() != WSA_IO_PENDING)   
			{   //WSA_IO_PENDING �� ����ķ���ֵ��˵��WSARecv�����ɹ��ˣ�����I/O������û�����
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
	
	ThreadQuitEventArray[0] = WSACreateEvent();    //����¼������������߳�֪ͨͨ���߳̽���   
	WSAResetEvent(ThreadQuitEventArray[0]);
	//����ͨ���߳�--socket �ͻ���
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

	//fAlertable ֵ���Ӧ��Ϊ FALSE;
	//�������ã�ֱ���ȴ�ͨ���߳̽���
	WSAWaitForMultipleEvents(1,ThreadQuitEventArray,TRUE,WSA_INFINITE,FALSE);
	return 0;
}

