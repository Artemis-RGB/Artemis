#include <memory>
#include <iostream>
#include <string>
#include <cstdio>

using namespace std;

template <typename ... Args>
string string_format(const std::string& format, Args ... args)
{
	size_t size = snprintf(nullptr, 0, format.c_str(), args ...) + 1; // Extra space for '\0'
	unique_ptr<char[]> buf(new char[size]);
	snprintf(buf.get(), size, format.c_str(), args ...);
	return string(buf.get(), buf.get() + size - 1); // We don't want the '\0' inside
}
