#pragma once
#include <string>

namespace first_game_project
{
    REGISTER_SCRIPT(character_script)

    class character_script : public pk::script::entity_script
    {
    public:
        constexpr explicit character_script(entity entity) : entity_script(entity)
        {
        }
        
        void update(float) override;
    };
}
