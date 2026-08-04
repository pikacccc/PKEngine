#pragma once
#include "ToolsCommon.h"

namespace pk::tools
{
    struct vertex
    {
        math::v4 tangents {};
        math::v3 position {};
        math::v3 normal {};
        math::v2 uv {};
    };

    struct mesh
    {
        //Initial data
        util::vector<math::v3>               positions;
        util::vector<math::v3>               normals;
        util::vector<math::v4>               tangents;
        util::vector<util::vector<math::v2>> uv_sets;
        util::vector<u32>                    raw_indices;

        //Intermediate data
        util::vector<vertex> vertices;
        util::vector<u32>    indices;
        //Output data
    };

    struct lod_group
    {
        std::string        name;
        util::vector<mesh> meshes;
    };

    struct scene
    {
        std::string             name;
        util::vector<lod_group> lod_groups;
    };

    struct geometry_import_settings
    {
        f32 smoothing_angle;
        u8  calculate_normals;
        u8  calculate_tangents;
        u8  reverse_handedness;
        u8  import_embedded_textures;
        u8  import_animations;
    };

    struct scene_data
    {
        u8*                      buffer;
        u32                      buffer_size;
        geometry_import_settings settings;
    };

    void process_scene(scene& scene, const geometry_import_settings& settings);
    void pack_data(scene& scene, scene_data& settings);
}
