#include "CharacterScript.h"

namespace first_game_project
{
    REGISTER_SCRIPT(character_script)
    
    void character_script::update(float x)
    {
        entity_script::update(x);
    }
}
