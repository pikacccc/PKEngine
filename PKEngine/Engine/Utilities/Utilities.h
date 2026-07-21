#pragma once

#define USE_STL_VECTOR 1
#define USE_STL_DEQUE 1

#if USE_STL_VECTOR
#include <algorithm>
#include <vector>
namespace pk::util {
	template<typename T>
	using vector = std::vector<T>;
	
	template<typename T>
	void erase_unordered(vector<T>& vec, size_t index)
	{
		assert(index < vec.size());

		if (index != vec.size() - 1)
		{
			vec[index] = std::move(vec.back());
		}

		vec.pop_back();
	}
}
#endif // USE_STL_VECTOR

#if USE_STL_DEQUE
#include <deque>
namespace pk::util {
	template<typename T>
	using deque = std::deque<T>;
}
#endif // USE_STL_DEQUE

namespace pk::util {
	//TODO: implement our own containers
}