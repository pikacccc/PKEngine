#pragma once
#include "../Common/CommonHeader.h"
#include "Window.h"

namespace pk::platform
{
    struct window_init_info;

    window create_window(const window_init_info* init_info = nullptr);
    void   remove_window(window_id id);
}
