#include "Transform.h"
#include "Entity.h"

namespace pk::transform {
	namespace {
		util::vector<math::v3> positions;
		util::vector<math::v4> rotations;
		util::vector<math::v3> scales;
	}

	component create_transform(const init_info& info, game_entity::entity entity) {
		assert(entity.is_valid());
		const id::id_type entity_index{ id::index(entity.get_id()) };
		if (positions.size() > entity_index) {
			positions[entity_index] = math::v3(info.position);
			rotations[entity_index] = math::v4(info.rotation);
			scales[entity_index] = math::v3(info.scale);
		}
		else
		{
			assert(positions.size() == entity_index);
			rotations.emplace_back(math::v4(info.rotation));
			positions.emplace_back(math::v3(info.position));
			scales.emplace_back(math::v3(info.scale));
		}

		return component(transform_id{ (id::id_type)positions.size() - 1 });
	}

	void remove_transform(component c) {
		assert(c.is_valid());
	}

	math::v3 component::position() const {
		assert(is_valid());
		return positions[id::index(_id)];
	}
	math::v4 component::rotation() const {
		assert(is_valid());
		return rotations[id::index(_id)];
	}
	math::v3 component::scale() const {
		assert(is_valid());
		return scales[id::index(_id)];
	}
}