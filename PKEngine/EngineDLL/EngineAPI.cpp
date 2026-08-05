#include "common.h"
#include "CommonHeader.h"
#include "../Engine/Components/Script.h"
#include "../Graphics/Renderer.h"
#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <Windows.h>
#include <atlsafe.h>

#include "Platform/PlatformTypes.h"

using namespace pk;

namespace
{
    HMODULE game_code_dll{ nullptr };
    using _get_script_creator = script::detail::script_creator(*)(size_t);
    _get_script_creator get_script_creator{ nullptr };
    using _get_script_names = LPSAFEARRAY(*)(void);
    _get_script_names get_script_names{ nullptr };

    util::vector<graphics::render_surface> render_surfaces;
}

EDITOR_INTERFACE u32
LoadGameCodeDll(const char* dll_path)
{
    if (game_code_dll) return FALSE;
    game_code_dll = LoadLibraryA(dll_path);
    assert(game_code_dll);

    get_script_creator = reinterpret_cast<_get_script_creator>(GetProcAddress(game_code_dll, "get_script_creator"));
    get_script_names   = reinterpret_cast<_get_script_names>(GetProcAddress(game_code_dll, "get_script_names"));

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
    int res{ FreeLibrary(game_code_dll) };
    assert(res);
    game_code_dll = nullptr;
    return TRUE;
}

EDITOR_INTERFACE script::detail::script_creator
GetScriptCreator(const char* name)
{
    return (game_code_dll && get_script_creator)
               ? get_script_creator(script::detail::string_hash()(name))
               : nullptr;
}

EDITOR_INTERFACE LPSAFEARRAY
GetScriptNames()
{
    return (game_code_dll && get_script_names) ? get_script_names() : nullptr;
}

EDITOR_INTERFACE u32
CreateRenderSurface(HWND host, s32 width, s32 height)
{
    assert(host);
    platform::window_init_info init_info{ nullptr, host, nullptr, 0, 0, width, height };
    graphics::render_surface   surface{ platform::create_window(&init_info), {} };
    assert(surface.window.is_valid());
    render_surfaces.emplace_back(surface);
    return static_cast<u32>(render_surfaces.size()) - 1;
}

EDITOR_INTERFACE void
RemoveRenderSurface(u32 id)
{
    assert(id<render_surfaces.size());
    platform::remove_window(render_surfaces[id].window.get_id());
    util::erase_unordered(render_surfaces, id);
}

EDITOR_INTERFACE HWND
GetRenderHandle(u32 id)
{
    assert(id < render_surfaces.size());
    return static_cast<HWND>(render_surfaces[id].window.handle());
}

EDITOR_INTERFACE void
ResizeRenderSurface(u32 id)
{
    assert(id<render_surfaces.size());
    render_surfaces[id].window.resize(0, 0);
}
