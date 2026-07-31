#pragma once
#include "..\Common\CommonHeader.h"
#include "..\Platform\Window.h"

namespace pk::graphics
{
    class surface
    {
    };

    struct render_surface
    {
        platform::window window{};
        surface surface{};
    };
}
