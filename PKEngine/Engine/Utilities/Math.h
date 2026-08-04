#pragma once

#include "CommonHeader.h"
#include "MathTypes.h"

namespace pk::math
{
    template <typename T>
    constexpr T clamp(T value, T min, T max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }
}
