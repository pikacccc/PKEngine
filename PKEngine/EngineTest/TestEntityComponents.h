#pragma once

#include "Test.h"
#include "..\Engine\Components\Entity.h"
#include "..\Engine\Components\Transform.h"

#include <iostream>
#include <ctime>


using namespace pk;

class engine_test : public test
{
public:
	bool initialize() override {
		srand((u32)time(nullptr));
		return true;
	}
	void run() override {
		do {
			for (u32 i{ 0 }; i < 10000; ++i) {
				create_random();
				remove_random();
				_num_entities = (u32)_entities.size();
			}
			print_results();
		} while (getchar() != 'q');
	}
	void shutdown() override {}

private:
	void create_random() {
		u32 count = rand() % 20;
		if (_entities.empty()) count = 1000;

		transform::init_info transform_info{};
		game_entity::entity_info entity_info{
			&transform_info,
		};

		while (count > 0)
		{
			++_added;
			game_entity::entity entity{ game_entity::create_game_entity(entity_info) };
			assert(entity.is_valid());
			_entities.push_back(entity);
			--count;
		}
	}

	void remove_random() {
		u32 count = rand() % 20;
		if (_entities.size() < 1000) return;
		while (count > 0)
		{
			const u32 index{ (u32)rand() % _entities.size() };
			const game_entity::entity entity{ _entities[index] };
			assert(entity.is_valid());
			if (entity.is_valid()) {
				game_entity::remove_game_entity(entity);
				_entities.erase(_entities.begin() + index);
				assert(!game_entity::is_alive(entity));
				_removed++;
			}
			--count;
		}

	}

	void print_results() {
		std::cout << "Entities created" << _added << "\n";
		std::cout << "Entities deleted" << _removed << "\n";
	}

	util::vector<game_entity::entity> _entities;

	u32 _added{ 0 };
	u32 _removed{ 0 };
	u32 _num_entities{ 0 };
};