// Original work by VRocker https://github.com/VRocker/LogiLed2Corsair
// I'm mainly a C# developer, and these modification aren't a piece of art, but it suits our needs.

// The MIT License (MIT)
// 
// Copyright (c) 2015 VRocker
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include "main.h"
#include <stdio.h>
#include <tchar.h>
#include <thread>
#include "LogiLedDefs.h"
#define WIN32_LEAN_AND_MEAN 
#include <Windows.h>
#include "Logger.h"

#include <complex>
#include <filesystem>


static bool g_hasInitialised = false;
const char* game = "";

void cleanup()
{
	CLogger::EndLogging();
}

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		{
			atexit(cleanup);

			CLogger::InitLogging("Log.txt");
			CLogger::SetLogLevel(LogLevel::None);

			// Get the process that loaded the DLL
			TCHAR divisionFind[] = _T("Division");
			TCHAR gtaFind[] = _T("GTA");
			TCHAR szPath[MAX_PATH];
			GetModuleFileName(NULL, szPath, MAX_PATH);

			if (_tcscmp(szPath, divisionFind) != 0)
				game = "division";
			else if (_tcscmp(szPath, gtaFind) != 0)
				game = "gta";

			CLogger::OutputLog("Attached to process.", LogLevel::Debug);
		}
		break;

	case DLL_PROCESS_DETACH:
		{
			cleanup();

			CLogger::OutputLog_s("Detached from process.", LogLevel::Debug);
		}
		break;
	}

	return true;
}

bool LogiLedInit()
{
	CLogger::OutputLog_s("Logitech LED init called.", LogLevel::Debug);
	g_hasInitialised = true;
	return true;
}

bool LogiLedGetSdkVersion(int* majorNum, int* minorNum, int* buildNum)
{
	CLogger::OutputLog("LogiLedGetSdkVersion called.", LogLevel::Debug);

	// Mimic the SDK version
	*majorNum = 8;
	*minorNum = 81;
	*buildNum = 15;

	return true;
}

bool LogiLedSetTargetDevice(int targetDevice)
{
	CLogger::OutputLog("LogiLedSetTargetDevice called (%i)", LogLevel::Debug, targetDevice);

	// Logitech SDK says this function returns false if LogiLedInit hasn't been called
	return g_hasInitialised;
}

bool LogiLedSaveCurrentLighting()
{
	CLogger::OutputLog("LogiLedSaveCurrentLighting called (%i)", LogLevel::Debug);
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
		CLogger::OutputLog("Could not open the pipe  - (error %i)", LogLevel::Debug, GetLastError());
		return;
	}

	DWORD cbWritten;
	WriteFile(hPipe1, msg, strlen(msg), &cbWritten, NULL);
	CLogger::OutputLog_s("Transmitted to pipe", LogLevel::Debug);
}

bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage)
{
	CLogger::OutputLog("LogiLedSetLighting called (%i %i %i)", LogLevel::Debug, redPercentage, greenPercentage, bluePercentage);

	std::ostringstream os;
	os << "0 0 " << redPercentage << " " << greenPercentage << " " << bluePercentage;
	Transmit(os.str().c_str());

	return true;
}

bool LogiLedRestoreLighting()
{
	CLogger::OutputLog("LogiLedRestoreLighting called", LogLevel::Debug);
	return true;
}

bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	CLogger::OutputLog("LogiLedFlashLighting called (%i %i %i %i %i)", LogLevel::Debug, redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);
	return true;
}

bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	CLogger::OutputLog("LogiLedPulseLighting called (%i %i %i %i %i)", LogLevel::Debug, redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);
	return true;
}

bool LogiLedStopEffects()
{
	CLogger::OutputLog_s("LogiLedStopEffects called", LogLevel::Debug);
	return true;
}

bool LogiLedSetLightingFromBitmap(unsigned char bitmap[])
{
	CLogger::OutputLog_s("LogiLedSetLightingFromBitmap called", LogLevel::Debug);
	return true;
}

bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	CLogger::OutputLog("LogiLedSetLightingForKeyWithScanCode called [Key: %i] (%i %i %i)", LogLevel::Debug, keyCode, redPercentage, greenPercentage, bluePercentage);
	return true;
}

bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	CLogger::OutputLog("LogiLedSetLightingForKeyWithHidCode called [Key: %i] (%i %i %i)", LogLevel::Debug, keyCode, redPercentage, greenPercentage, bluePercentage);
	return true;
}

bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	CLogger::OutputLog("LogiLedSetLightingForKeyWithQuartzCode called [Key: %i] (%i %i %i)", LogLevel::Debug, keyCode, redPercentage, greenPercentage, bluePercentage);
	return true;
}

bool LogiLedSetLightingForKeyWithKeyName(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage)
{
	CLogger::OutputLog("LogiLedSetLightingForKeyWithKeyName called [Key: %i] (%i %i %i)", LogLevel::Debug, keyName, redPercentage, greenPercentage, bluePercentage);

	// Only transmit interesting keys. This can most likely be done prettier, but I'm no C++ dev.
	if (game == "division" &&
		(keyName == LogiLed::F1 ||
			keyName == LogiLed::F2 ||
			keyName == LogiLed::F3 ||
			keyName == LogiLed::F4 ||
			keyName == LogiLed::R ||
			keyName == LogiLed::G ||
			keyName == LogiLed::V)
	)
	{
		std::ostringstream os;
		os << "1 " << keyName << " " << redPercentage << " " << greenPercentage << " " << bluePercentage;
		std::string s = os.str();
		Transmit(os.str().c_str());
	}
	return true;
}

bool LogiLedSaveLightingForKey(LogiLed::KeyName keyName)
{
	CLogger::OutputLog("LogiLedSaveLightingForKey called [Key: %i]", LogLevel::Debug, keyName);
	return true;
}

bool LogiLedRestoreLightingForKey(LogiLed::KeyName keyName)
{
	CLogger::OutputLog("LogiLedRestoreLightingForKey called [Key: %i]", LogLevel::Debug, keyName);
	return true;
}

bool LogiLedFlashSingleKey(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval)
{
	CLogger::OutputLog("LogiLedFlashSingleKey called [Key: %i] (%i %i %i %i %i)", LogLevel::Debug, keyName, redPercentage, greenPercentage, bluePercentage, msDuration, msInterval);
	return true;
}

bool LogiLedPulseSingleKey(LogiLed::KeyName keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite)
{
	CLogger::OutputLog("LogiLedPulseSingleKey called [Key: %i] (%i %i %i %i %i %i %i %i)", LogLevel::Debug, keyName, startRedPercentage, startGreenPercentage, startBluePercentage, finishRedPercentage, finishGreenPercentage, finishBluePercentage, msDuration, isInfinite);
	return true;
}

bool LogiLedStopEffectsOnKey(LogiLed::KeyName keyName)
{
	CLogger::OutputLog("LogiLedStopEffectsOnKey called [Key: %i]", LogLevel::Debug, keyName);
	return true;
}

void LogiLedShutdown()
{
	CLogger::OutputLog_s("LogiLedShutdown called.", LogLevel::Debug);
	g_hasInitialised = false;
}

