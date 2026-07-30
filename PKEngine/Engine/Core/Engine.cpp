#ifndef SHIPPING

#include "..\Content\ContentLoader.h"
#include "..\Components\Script.h"
#include <thread>

bool engine_initialize()
{
    bool res{pk::content::load_game()};
    return res;
}

void engine_update()
{
    pk::script::update(10.f);
    std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void engine_shutdown()
{
    pk::content::unload_game();
}

#endif
