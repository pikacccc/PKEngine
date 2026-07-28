#include "common.h"
#include "CommonHeader.h"

#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAN
#endif

#include <Windows.h>

using namespace pk;

namespace
{
    HMODULE game_code_dll{nullptr};
}

EDITOR_INTERFACE u32
LoadGameCodeDll(const char* dll_path)
{
    if (game_code_dll) return FALSE;
    game_code_dll = LoadLibraryA(dll_path);
    assert(game_code_dll);

    return game_code_dll ? TRUE : FALSE;
}

EDITOR_INTERFACE u32
UnloadGameCodeDll()
{
    if (!game_code_dll) return FALSE;
    assert(game_code_dll);
    int res{FreeLibrary(game_code_dll)};
    assert(res);
    game_code_dll = nullptr;
    return TRUE;
}
