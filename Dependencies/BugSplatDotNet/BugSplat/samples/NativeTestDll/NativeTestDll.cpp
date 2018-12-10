// NativeTestDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

extern "C" __declspec(dllexport) void MemoryException(void)
{
    *(int *)0 = 0;
}

