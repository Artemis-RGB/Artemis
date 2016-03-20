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

#include "Logger.h"

#include <time.h>
#include <stdlib.h>
#include <stdarg.h>


// Ok this looks butt-ugleh but its just to force the default log level to debug if we compile in debug
#ifdef _DEBUG
LogLevel CLogger::m_eLogLevel = LogLevel::Debug;
#else
LogLevel CLogger::m_eLogLevel = LogLevel::All;
#endif
FILE* CLogger::m_pFile = 0;

void CLogger::InitLogging(const char* szFile)
{
	if (!m_pFile)
	{
		m_pFile = fopen(szFile, "a+");
		if (!m_pFile)
		{
			// Hum, we couldn't open the file for writing
			printf("ERROR: Unable to open log file %s.\n", szFile);
		}
	}
}

void CLogger::EndLogging(void)
{
	if (m_pFile)
	{
		fclose(m_pFile);
		m_pFile = 0;
	}
}

void CLogger::OutputLog_s(const char* sz, const LogLevel eLevel)
{
	// If the level of this log entry is less important than what we are told to log, just return
	if (eLevel < m_eLogLevel)
		return;

	// If we don't have a file to write to, bail.
	if (!m_pFile)
		return;

	char szOutput[512] = {0};

	time_t rawtime;
	struct tm* timeinfo;

	time(&rawtime);
	timeinfo = localtime(&rawtime);

	// Timestamp the log entry
	size_t uiLen = sprintf(szOutput, "<%.2d/%.2d/%.2d - %.2d:%.2d:%.2d> %s\n", timeinfo->tm_mday, timeinfo->tm_mon + 1, timeinfo->tm_year + 1900, timeinfo->tm_hour, timeinfo->tm_min, timeinfo->tm_sec, sz);

	// Write the text to the file
	fwrite(szOutput, 1, uiLen, m_pFile);
	// Flush the log file to the disk. May move this to a seperate function
	fflush(m_pFile);

	// Output the text to any console that may be attached
	printf("%s", szOutput);
}

void CLogger::OutputLog(const char* sz, const LogLevel eLevel, ...)
{
	char szText[1024] = {0};
	va_list marker;
	va_start(marker, eLevel);
	vsprintf(szText, sz, marker);
	va_end(marker);

	// Since this function is pretty much the same as the safer function, we'll just redirect it there
	OutputLog_s(szText, eLevel);
}

