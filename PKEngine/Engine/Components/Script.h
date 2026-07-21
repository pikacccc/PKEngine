#pragma once
#include "ComponentsCommon.h"

namespace pk::script {

    struct init_info
    {
        detail::script_creator script_creator;
    };

    component create(const init_info& info, game_entity::entity entity);
    void remove(const component c);
}