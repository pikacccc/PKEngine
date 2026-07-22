#include "Script.h"
#include "Entity.h"

namespace pk::script
{
    namespace
    {
        util::vector<detail::script_ptr> entity_scripts;
        util::vector<id::id_type> id_mapping;

        util::vector<id::generation_type> generations;
        util::deque<script_id> free_ids;

        using script_registery = std::unordered_map<u8, detail::script_creator>;

        script_registery& registry()
        {
            static script_registery reg;
            return reg;
        }

        bool exists(script_id id)
        {
            assert(id::is_valid(id));
            const id::id_type index{id::index(id)};
            assert(index<generations.size() && id_mapping[index]<entity_scripts.size());
            return generations[index] == id::generation(id)
                && entity_scripts[id_mapping[index]] && entity_scripts[id_mapping[index]]->is_valid();
        }
    }


    namespace detail
    {
        u8 register_script(size_t tag, script_creator func)
        {
            bool res{registry().insert(script_registery::value_type{tag, func}).second};
            assert(res);
            return res;
        }
    }

    component create(const init_info& info, game_entity::entity entity)
    {
        assert(entity.is_valid());
        assert(info.script_creator);
        script_id id{};
        if (free_ids.size() > id::min_deleted_elements)
        {
            id = free_ids.front();
            assert(!exists(id));
            free_ids.pop_back();
            id = script_id{id::new_generation(id)};
            ++generations[id::index(id)];
        }
        else
        {
            id = script_id{static_cast<id::id_type>(id_mapping.size())};
            id_mapping.emplace_back();
            generations.push_back(0);
        }

        assert(id::is_valid(id));
        entity_scripts.emplace_back(info.script_creator(entity));
        assert(entity_scripts.back()->get_id()==entity.get_id());
        const id::id_type index{static_cast<id::id_type>(entity_scripts.size())};
        id_mapping[id::index(id)] = index;

        return component{id};
    }

    void remove(const component c)
    {
        assert(c.is_valid() && exists(c.get_id()));
        const script_id id{c.get_id()};
        const id::id_type script_ptr_index{id_mapping[id::index(id)]};
        const script_id last_id{entity_scripts.back()->script().get_id()};
        util::erase_unordered(entity_scripts, script_ptr_index);
        id_mapping[id::index(last_id)] = script_ptr_index;
        id_mapping[id::index(id)] = id::invalid_id;
    }
}
