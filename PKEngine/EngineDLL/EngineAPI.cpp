#include "common.h"
#include "CommonHeader.h"
#include "..\Engine\Components\Script.h"

#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAN
#endif

#include <Windows.h>

using namespace pk;

namespace
{
    HMODULE game_code_dll{nullptr};
    using _get_script_creator = pk::script::detail::script_creator(*)(size_t);
    _get_script_creator get_script_creator{nullptr};
    using _get_script_names = LPSAFEARRAY(*)(void);
    _get_script_names get_script_names{nullptr};
}

EDITOR_INTERFACE u32
LoadGameCodeDll(const char* dll_path)
{
    if (game_code_dll) return FALSE;
    game_code_dll = LoadLibraryA(dll_path);
    assert(game_code_dll);

    get_script_creator = reinterpret_cast<_get_script_creator>(GetProcAddress(game_code_dll, "get_script_creator"));
    get_script_names = reinterpret_cast<_get_script_names>(GetProcAddress(game_code_dll, "get_script_names"));

    return game_code_dll
           && get_script_creator
           && get_script_names
               ? TRUE
               : FALSE;
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

EDITOR_INTERFACE pk::script::detail::script_creator
GetScriptCreator(const char* name)
{
    return (game_code_dll && get_script_creator) ? get_script_creator(pk::script::detail::string_hash()(name)) : nullptr;
}

EDITOR_INTERFACE LPSAFEARRAY
GetScriptNames()
{
    return (game_code_dll && get_script_names) ? get_script_names() : nullptr; 
}