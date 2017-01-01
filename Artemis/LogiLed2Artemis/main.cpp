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
#include <complex>
#include <filesystem>
#include <fstream>
using namespace std;

static bool g_hasInitialised = false;
static bool mustLog = false;
static int throttle = 0;
const char* game = "";


const char* GetGame()
{
	CHAR divisionFind[] = ("Division");
	CHAR gtaFind[] = ("GTA");
	CHAR szPath[MAX_PATH];

	GetModuleFileNameA(NULL, szPath, MAX_PATH);
	char *output;

	output = strstr(szPath, divisionFind);
	if (output)
		return "division";
	output = strstr(szPath, gtaFind);
	if (output)
		return "gta";

	return "bf1";
};

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		game = GetGame();

		if (mustLog)
		{
			ofstream myfile;
			myfile.open("log.txt", ios::out | ios::app);
			myfile << "Main called, DLL loaded into " << game << "\n";
			myfile.close();
		}
	}
	return true;
}

bool LogiLedInit()
{
	g_hasInitialised = true;
	return true;
}

bool LogiLedGetSdkVersion(int* majorNum, int* minorNum, int* buildNum)
{
	// Mimic the SDK version
	*majorNum = 8;
	*minorNum = 81;
	*buildNum = 15;

	return true;
}

bool LogiLedSetTargetDevice(int targetDevice)
{
	// Logitech SDK says this function returns false if LogiLedInit hasn't been called
	return g_hasInitialised;
}

bool LogiLedSaveCurrentLighting()
{
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

// LogiLedSetLighting appears to have an undocumented extra argument
bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage, int custom = 0)
{
	// GTA goes mental on the SetLighting calls, lets only send one in every 20
	if (game == "gta")
	{
		throttle++;
		if (throttle > 20)
			throttle = 0;
		else
			return true;
	}

	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		if (custom == 0)
		{
			myfile << "LogiLedSetLighting called\n";
		}
		else
		{
			myfile << "LogiLedSetLighting called with custom " << custom << "\n";
		}
		myfile.close();
	}
	ostringstream os;
	os << "0 " << hex << custom << " " << dec << redPercentage << " " << greenPercentage << " " << bluePercentage;
	Transmit(os.str().c_str());

	return true;
}

bool LogiLedRestoreLighting()
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedRestoreLighting called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedFlashLighting called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedPulseLighting called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedStopEffects()
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedStopEffects called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedSetLightingFromBitmap(unsigned char bitmap[])
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSetLightingFromBitmap called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSetLightingForKeyWithScanCode called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSetLightingForKeyWithHidCode called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSetLightingForKeyWithQuartzCode called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedSetLightingForKeyWithKeyName(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSetLightingForKeyWithKeyName called\n";
		myfile.close();
	}

	// Only transmit interesting keys. This can most likely be done prettier, but I'm no C++ dev.
	if (game == "division" && (keyName == LogiLed::F1 || keyName == LogiLed::F2 || keyName == LogiLed::F3 || keyName == LogiLed::F4 || keyName == LogiLed::R || keyName == LogiLed::G || keyName == LogiLed::V))
	{
		ostringstream os;
		os << "1 " << keyName << " " << redPercentage << " " << greenPercentage << " " << bluePercentage;
		string s = os.str();
		Transmit(os.str().c_str());
		return true;
	}

	ostringstream os;
	os << "1 " << keyName << " " << redPercentage << " " << greenPercentage << " " << bluePercentage;
	string s = os.str();
	Transmit(os.str().c_str());
	return true;
}

bool LogiLedSaveLightingForKey(LogiLed::KeyName keyName)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedSaveLightingForKey called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedRestoreLightingForKey(LogiLed::KeyName keyName)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedRestoreLightingForKey called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedFlashSingleKey(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedFlashSingleKey called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedPulseSingleKey(LogiLed::KeyName keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedPulseSingleKey called\n";
		myfile.close();
	}

	return true;
}

bool LogiLedStopEffectsOnKey(LogiLed::KeyName keyName)
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedStopEffectsOnKey called\n";
		myfile.close();
	}

	return true;
}

void LogiLedShutdown()
{
	if (mustLog)
	{
		ofstream myfile;
		myfile.open("log.txt", ios::out | ios::app);
		myfile << "LogiLedShutdown called\n";
		myfile.close();
	}

	g_hasInitialised = false;
}