#pragma once

#define USE_STL_VECTOR 1
#define USE_STL_DEQUE 1

#if USE_STL_VECTOR
#include <vector>
namespace pk::util {
	template<typename T>
	using vector = std::vector<T>;
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