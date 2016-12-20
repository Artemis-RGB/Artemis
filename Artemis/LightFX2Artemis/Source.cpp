#include <Windows.h>
#include <stdio.h>
#include <thread>
#include <complex>
#include <filesystem>
#include <fstream>
#include "LightFxState.h"
#include "LFX2.h"
#include "LFXDecl.h"
#include "log.h"

using namespace std;

LightFxState* lightFxState;

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		// Get executing .exe path. Not very pretty but other methods messed JSON up
		WCHAR ownPth[MAX_PATH];
		HMODULE hModule = GetModuleHandle(NULL);
		GetModuleFileName(hModule, ownPth, sizeof ownPth);
		char* moduleName = new char[sizeof ownPth];
		wcstombs(moduleName, ownPth, sizeof ownPth);

		lightFxState = new LightFxState(moduleName);
		FILELog::ReportingLevel() = logDEBUG1;
		FILE* log_fd = fopen("log.txt", "w");
		Output2FILE::Stream() = log_fd;

		FILE_LOG(logDEBUG1) << "Main called, DLL loaded into " << lightFxState->Game;
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
		FILE_LOG(logDEBUG1) << "Could not open the pipe - " << GetLastError();
		return;
	}

	DWORD cbWritten;
	WriteFile(hPipe1, msg, strlen(msg), &cbWritten, NULL);
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Initialize()
{
	FILE_LOG(logDEBUG1) << "Called LFX_Initialize()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Release()
{
	FILE_LOG(logDEBUG1) << "Called LFX_Release()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Reset()
{
	FILE_LOG(logDEBUG1) << "Called LFX_Reset()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Update()
{
	FILE_LOG(logDEBUG1) << "Called LFX_Update()";

	// Get the JSON
	json j = lightFxState->GetJson();
	// Transmit to Artemis
	Transmit(j.dump().c_str());

	// Only bother dumping it indented if actually debugging
	if (FILELog::ReportingLevel() == logDEBUG1)	
		FILE_LOG(logDEBUG1) << "JSON: " << j.dump(4);

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_UpdateDefault()
{
	FILE_LOG(logDEBUG1) << "Called LFX_UpdateDefault()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumDevices(unsigned int* const numDevices)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetNumDevices()";

	// Keyboard, mouse, headset, mousemat, generic
	*numDevices = 5;
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetDeviceDescription(const unsigned int devIndex, char* const devDesc, const unsigned int devDescSize, unsigned char* const devType)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetDeviceDescription()";

	stringstream ss;
	ss << "Device " << devIndex;
	string deviceName = ss.str();
	if (deviceName.length() > devDescSize)
		return LFX_ERROR_BUFFSIZE;

	// Just assign similar LFX device types to each index
	sprintf_s(devDesc, devDescSize, deviceName.c_str());
	if (devIndex == 0)
		*devType = LFX_DEVTYPE_KEYBOARD;
	else if (devIndex == 1)
		*devType = LFX_DEVTYPE_MOUSE;
	else if (devIndex == 2)
		*devType = LFX_DEVTYPE_SPEAKER;
	else if (devIndex == 3)
		*devType = LFX_DEVTYPE_GAMEPAD;
	else
		*devType = LFX_DEVTYPE_DESKTOP;

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumLights(const unsigned int devIndex, unsigned int* const numLights)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetNumLights()";

	// Just do one for now
	*numLights = 5;
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightDescription(const unsigned int devIndex, const unsigned int lightIndex, char* const lightDesc, const unsigned int lightDescSize)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetLightDescription()";

	stringstream ss;
	ss << "Device " << devIndex << "Light" << lightIndex;
	string lightDescription = ss.str();
	if (lightDescription.length() > lightDescSize)
		return LFX_ERROR_BUFFSIZE;

	sprintf_s(lightDesc, lightDescSize, lightDescription.c_str());
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightLocation(const unsigned int devIndex, const unsigned int lightIndex, PLFX_POSITION const lightLoc)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetLightLocation()";

	*lightLoc = LFX_POSITION{0,0,0};
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightColor(const unsigned int devIndex, const unsigned int lightIndex, PLFX_COLOR const lightCol)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetLightColor()";

	*lightCol = *lightFxState->Devices[devIndex]->Lights[lightIndex]->Color;
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightColor(const unsigned int devIndex, const unsigned int lightIndex, const PLFX_COLOR lightCol)
{
	FILE_LOG(logDEBUG1) << "Called LFX_SetLightColor()";

	lightFxState->Devices[devIndex]->Lights[lightIndex]->Color = lightCol;
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Light(const unsigned int locationMask, const unsigned int colorVal)
{
	FILE_LOG(logDEBUG1) << "Called LFX_Light(locationMask " << locationMask << ", colorVal " << colorVal << ")";

	lightFxState->LocationMask = locationMask;
	lightFxState->LocationMaskLight->FromInt(colorVal);
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColor(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR)
{
	FILE_LOG(logDEBUG1) << "Called LFX_SetLightActionColor()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColorEx(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR, const PLFX_COLOR)
{
	FILE_LOG(logDEBUG1) << "Called LFX_SetLightActionColorEx()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColor(const unsigned int, const unsigned int, const unsigned int)
{
	FILE_LOG(logDEBUG1) << "Called LFX_ActionColor()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColorEx(const unsigned int, const unsigned int, const unsigned int, const unsigned int)
{
	FILE_LOG(logDEBUG1) << "Called LFX_ActionColorEx()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetTiming(const int)
{
	FILE_LOG(logDEBUG1) << "Called LFX_SetTiming()";
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetVersion(char* const version, const unsigned int versionSize)
{
	FILE_LOG(logDEBUG1) << "Called LFX_GetVersion()";

	sprintf_s(version, versionSize, "2.0.0.0");
	return LFX_SUCCESS;
}
