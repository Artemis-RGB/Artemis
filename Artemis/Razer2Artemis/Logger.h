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

#pragma once
#ifndef _LOGGER_H
#define _LOGGER_H

#include <stdio.h>

enum class LogLevel
{
	Debug,
	All,
	Critical,
	Warning,
	Information,
	User,
	Internal,
	None,

	// Just to force the compiler to treat the enum options as an int
	FORCE_32BIT = 0x7fffffff
};

class CLogger
{
public:
	static LogLevel GetLogLevel(void)
	{
		return m_eLogLevel;
	}

	static void SetLogLevel(const LogLevel e)
	{
#ifndef _DEBUG
		if (e == LogLevel::Debug) return;
#endif
		m_eLogLevel = e;
	}

	static void InitLogging(const char* szFile);
	// Always remember to end logging when you are finished otherwise you will have file handles floating around
	static void EndLogging(void);

	// Output the text to a log file.
	static void OutputLog_s(const char* sz, const LogLevel eLevel);
	static void OutputLog(const char* sz, const LogLevel eLevel, ...);

	static FILE* GetFile()
	{
		return m_pFile;
	}

private:
	static LogLevel m_eLogLevel;
	static FILE* m_pFile;
};

#endif

