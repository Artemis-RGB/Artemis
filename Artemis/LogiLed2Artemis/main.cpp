#include "main.h"
#include <stdio.h>
#include <tchar.h>
#include <thread>
#define WIN32_LEAN_AND_MEAN 
#include <Windows.h>
#include <complex>
#include <filesystem>
#include <fstream>
#include "../LightFX2Artemis/LightFxState.h"
#include "LFX2.h"

using namespace std;


static bool mustLog = false;
LightFxState* lightFxState;

char* GetGame()
{
	CHAR szPath[MAX_PATH];
	GetModuleFileNameA(NULL, szPath, MAX_PATH);

	return szPath;
};

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		lightFxState = new LightFxState(GetGame());
		if (mustLog)
		{
			ofstream myfile;
			myfile.open("log.txt", ios::out | ios::app);
			myfile << "Main called, DLL loaded into " << lightFxState->Game << "\n";
			myfile.close();
		}
	}
	return true;
}

void Transmit(const char* msg)
{
	//Pipe Init Data
	HANDLE hPipe1;
	char buf[100];
	LPTSTR lpszPipename1 = TEXT("\\\\.\\pipe\\artemis");

	hPipe1 = CreateFile(lpszPipename1, GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);
	if (hPipe1 == NULL || hPipe1 == INVALID_HANDLE_VALUE)
	{
		if (mustLog)
		{
			ofstream myfile;
			myfile.open("log.txt", ios::out | ios::app);
			myfile << "Could not open the pipe - " << GetLastError() << "\n";
			myfile.close();
		}
		return;
	}

	DWORD cbWritten;
	WriteFile(hPipe1, msg, strlen(msg), &cbWritten, NULL);
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Initialize()
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Release()
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Reset()
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Update()
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_UpdateDefault()
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumDevices(unsigned int* const)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetDeviceDescription(const unsigned int, char* const, const unsigned int, unsigned char* const)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumLights(const unsigned int, unsigned int* const)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightDescription(const unsigned int, const unsigned int, char* const, const unsigned int)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightLocation(const unsigned int, const unsigned int, PLFX_POSITION const)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightColor(const unsigned int, const unsigned int, PLFX_COLOR const)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightColor(const unsigned int, const unsigned int, const PLFX_COLOR)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Light(const unsigned int, const unsigned int)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColor(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColorEx(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR, const PLFX_COLOR) 
{ 
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColor(const unsigned int, const unsigned int, const unsigned int)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColorEx(const unsigned int, const unsigned int, const unsigned int, const unsigned int)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetTiming(const int)
{
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetVersion(char* const, const unsigned int)
{
	return LFX_SUCCESS;
}
