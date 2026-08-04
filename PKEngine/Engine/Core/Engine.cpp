#ifndef SHIPPING

#include "../Content/ContentLoader.h"
#include "../Components/Script.h"
#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"
#include "../Graphics/Renderer.h"
#include <thread>

using namespace pk;

namespace
{
    graphics::render_surface game_window{};

    LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wpram, LPARAM lpram)
    {
        switch (msg)
        {
            case WM_DESTROY:
                {
                    if (game_window.window.is_closed())
                    {
                        PostQuitMessage(0);
                        return 0;
                    }
                }
                break;
            case WM_SYSCHAR:
                if (wpram == VK_RETURN && (HIWORD(lpram) & KF_ALTDOWN))
                {
                    game_window.window.set_fullscreen(!game_window.window.is_fullscreen());
                    return 0;
                }
        }

        return DefWindowProc(hwnd, msg, wpram, lpram);
    }
}

bool engine_initialize()
{
    bool                       res{content::load_game()};
    platform::window_init_info info{
        &win_proc, nullptr, L"PK Game"
    };

    game_window.window = platform::create_window(&info);
    if (!game_window.window.is_valid()) return false;
    return res;
}

void engine_update()
{
    script::update(10.f);
    std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void engine_shutdown()
{
    platform::remove_window(game_window.window.get_id());
    content::unload_game();
}

#endif
