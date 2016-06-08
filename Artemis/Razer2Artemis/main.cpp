#include "main.h"
#include <stdio.h>
#include <tchar.h>
#include <Windows.h>
#include <filesystem>

TCHAR szName[] = TEXT("overwatchMmf");
TCHAR szMsg[] = TEXT("Message from first process.");

#define WIN32_LEAN_AND_MEAN 
#define BUF_SIZE 4096

static bool g_hasInitialised = false;
const char* game = "";


HANDLE hMapFile;

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		// Get the process that loaded the DLL
		TCHAR overwatchFind[] = _T("Overwatch");
		TCHAR szPath[MAX_PATH];
		GetModuleFileName(nullptr, szPath, MAX_PATH);

		if (_tcscmp(szPath, overwatchFind) != 0)
			game = "overwatch";

		// Setup mmf
		hMapFile = CreateFileMapping(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, BUF_SIZE, szName);
	}

	return true;
}

template <typename ... Args>
std::string string_format(const std::string& format, Args ... args)
{
	size_t size = snprintf(nullptr, 0, format.c_str(), args ...) + 1; // Extra space for '\0'
	std::unique_ptr<char[]> buf(new char[size]);
	snprintf(buf.get(), size, format.c_str(), args ...);
	return std::string(buf.get(), buf.get() + size - 1); // We don't want the '\0' inside
}


void WriteMmf(std::string msg)
{
	if (hMapFile == nullptr)
	{
		return;
	}

	LPCTSTR pBuf;
	pBuf = static_cast<LPTSTR>(MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, BUF_SIZE));

	if (pBuf == nullptr)
	{
		CloseHandle(hMapFile);
		return;
	}

	CopyMemory((PVOID)pBuf, msg.c_str(), msg.size());
	UnmapViewOfFile(pBuf);
}

RZRESULT Init()
{
	g_hasInitialised = true;
	return 0;
}

RZRESULT UnInit()
{
	return 0;
}

RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	return 0;
}

RZRESULT CreateKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	std::string res = "";
	if (Effect == Keyboard::CHROMA_CUSTOM)
	{
		res += "0|";
		auto keys = *static_cast<struct Keyboard::CUSTOM_EFFECT_TYPE*>(pParam);

		for (auto y = 0; y < 6; y++)
		{
			for (auto x = 0; x < 22; x++)
			{
				auto r = GetRValue(keys.Color[y][x]);
				auto g = GetGValue(keys.Color[y][x]);
				auto b = GetBValue(keys.Color[y][x]);
				res += string_format(" %d,%d,%d,%d,%d", y, x, r, g, b);
			}
		}
	}
	else if (Effect == Keyboard::CHROMA_CUSTOM_KEY)
	{
		res += "1|";
		auto keys = *static_cast<struct Keyboard::CUSTOM_KEY_EFFECT_TYPE*>(pParam);

		for (auto y = 0; y < 6; y++)
		{
			for (auto x = 0; x < 22; x++)
			{
				auto r = GetRValue(keys.Color[y][x]);
				auto g = GetGValue(keys.Color[y][x]);
				auto b = GetBValue(keys.Color[y][x]);
				res += string_format(" %d,%d,%d,%d,%d", y, x, r, g, b);
			}
		}
	}

	WriteMmf(res);
	return 0;
}

RZRESULT CreateMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	return 0;
}

RZRESULT CreateHeadsetEffect(ChromaSDK::Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	return 0;
}

RZRESULT CreateMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	return 0;
}

RZRESULT CreateKeypadEffect(ChromaSDK::Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	return 0;
}

RZRESULT DeleteEffect(RZEFFECTID EffectId)
{
	return 0;
}

RZRESULT SetEffect(RZEFFECTID EffectId)
{
	return 0;
}

RZRESULT RegisterEventNotification(HWND hWnd)
{
	return 0;
}

RZRESULT UnregisterEventNotification()
{
	return 0;
}

RZRESULT QueryDevice(RZDEVICEID DeviceId, DEVICE_INFO_TYPE& DeviceInfo)
{
	return 0;
}
