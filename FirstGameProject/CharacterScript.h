#pragma once
#include <string>

namespace first_game_project
{
    class character_script : public pk::script::entity_script
    {
    public:
        constexpr explicit character_script(entity entity) : entity_script(entity)
        {
        }
        
        void update(float) override;
    };
}
