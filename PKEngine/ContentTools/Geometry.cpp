#include "Geometry.h"

namespace pk::tools
{
    namespace
    {
        using namespace math;
        using namespace DirectX;

        void recalculate_normals(mesh& mesh)
        {
            const u32 num_indices { static_cast<u32>(mesh.raw_indices.size()) };
            mesh.normals.resize(num_indices);
            for (u32 i { 0 }; i < num_indices; ++i)
            {
                const u32 i0 { (u32)mesh.raw_indices[i] };
                const u32 i1 { (u32)mesh.raw_indices[++i] };
                const u32 i2 { (u32)mesh.raw_indices[++i] };

                XMVECTOR v0 { XMLoadFloat3(&mesh.positions[i0]) };
                XMVECTOR v1 { XMLoadFloat3(&mesh.positions[i1]) };
                XMVECTOR v2 { XMLoadFloat3(&mesh.positions[i2]) };

                XMVECTOR e0 { v1 - v0 };
                XMVECTOR e1 { v2 - v0 };
                XMVECTOR n { XMVector3Normalize(XMVector3Cross(e0, e1)) };

                XMStoreFloat3(&mesh.normals[i], n);
                mesh.normals[i - 1] = mesh.normals[i];
                mesh.normals[i - 2] = mesh.normals[i];
            }
        }

        void process_normals(mesh& mesh, f32 smoothing_angle)
        {
            const f32  cos_alpha { XMScalarCos(pi - smoothing_angle * pi / 180.0f) };
            const bool is_hard_edge { XMScalarNearEqual(smoothing_angle, 180.0f, epsilon) };
            const bool is_soft_edge { XMScalarNearEqual(smoothing_angle, 0.0f, epsilon) };
            const u32  num_indices { static_cast<u32>(mesh.raw_indices.size()) };
            const u32  num_vertices { static_cast<u32>(mesh.positions.size()) };

            assert(num_indices && num_vertices);

            mesh.indices.resize(num_indices);

            util::vector<util::vector<u32>> idx_ref(num_vertices);
            for (u32 i { 0 }; i < num_indices; ++i)
            {
                idx_ref[mesh.raw_indices[i]].emplace_back(i);
            }

            for (u32 i { 0 }; i < num_vertices; ++i)
            {
                auto& refs { idx_ref[i] };
                u32   num_refs { static_cast<u32>(refs.size()) };

                for (u32 j { 0 }; j < num_refs; ++j)
                {
                    mesh.indices[refs[j]] = static_cast<u32>(mesh.vertices.size());
                    vertex& v { mesh.vertices.emplace_back() };
                    v.position = mesh.positions[mesh.raw_indices[refs[j]]];

                    XMVECTOR n1 { XMLoadFloat3(&mesh.normals[refs[j]]) };
                    if (!is_hard_edge)
                    {
                        for (u32 k { j + 1 }; k < num_refs; ++k)
                        {
                            f32      cos_theta { 0.0f };
                            XMVECTOR n2 { XMLoadFloat3(&mesh.normals[refs[k]]) };
                            if (!is_soft_edge)
                            {
                                XMStoreFloat(&cos_theta, XMVector3Dot(n1, n2) * XMVector3ReciprocalLength(n1));
                            }

                            if (is_soft_edge || cos_theta >= cos_alpha)
                            {
                                n1 += n2;

                                mesh.indices[refs[k]] = mesh.indices[refs[j]];
                                refs.erase(refs.begin() + k);
                                --num_refs;
                                --k;
                            }
                        }
                    }
                    XMStoreFloat3(&v.normal, XMVector3Normalize(n1));
                }
            }
        }

        void process_uvs(mesh& mesh)
        {
            util::vector<vertex> old_vertices;
            old_vertices.swap(mesh.vertices);

            util::vector<u32> old_indices(mesh.indices.size());
            old_indices.swap(mesh.indices);

            const u32 num_indices { static_cast<u32>(old_indices.size()) };
            const u32 num_vertices { static_cast<u32>(old_vertices.size()) };

            util::vector<util::vector<u32>> idx_ref(num_vertices);
            for (u32 i { 0 }; i < num_indices; ++i)
            {
                idx_ref[old_indices[i]].emplace_back(i);
            }
            for (u32 i { 0 }; i < num_vertices; ++i)
            {
                auto& refs { idx_ref[i] };
                u32   num_refs { static_cast<u32>(refs.size()) };
                for (u32 j { 0 }; j < num_refs; ++j)
                {
                    mesh.indices[refs[j]] = static_cast<u32>(mesh.vertices.size());
                    vertex& v { old_vertices[old_indices[refs[j]]] };
                    v.uv = mesh.uv_sets[0][refs[j]];
                    mesh.vertices.emplace_back(v);

                    for (u32 k { j + 1 }; k < num_refs; ++k)
                    {
                        v2& uv1 { mesh.uv_sets[0][refs[k]] };
                        if (XMScalarNearEqual(v.uv.x, uv1.x, epsilon) &&
                            XMScalarNearEqual(v.uv.y, uv1.y, epsilon))
                        {
                            mesh.indices[refs[k]] = mesh.indices[refs[j]];
                            refs.erase(refs.begin() + k);
                            --num_refs;
                            --k;
                        }
                    }
                }
            }
        }

        void process_vertices_static(mesh& mesh)
        {
            const u32 num_vertices { static_cast<u32>(mesh.vertices.size()) };
            assert(num_vertices);
            mesh.packed_vertices_statics.reserve(num_vertices);

            for (u32 i { 0 }; i < num_vertices; ++i)
            {
                vertex&   v { mesh.vertices[i] };
                const u8  signs { static_cast<u8>((v.normal.z > 0.0f) << 1) };
                const u16 normal_x { static_cast<u16>(pack_float<16>(v.normal.x, -1.0, 1.0f)) };
                const u16 normal_y { static_cast<u16>(pack_float<16>(v.normal.y, -1.0, 1.0f)) };

                mesh.packed_vertices_statics.emplace_back(packed_vertex::vertex_static {
                                                              v.position,
                                                              { 0, 0, 0 },
                                                              signs,
                                                              { normal_x, normal_y },
                                                              {},
                                                              v.uv
                                                          });
            }
        }

        void process_vertices(mesh& mesh, const geometry_import_settings& settings)
        {
            assert((mesh.raw_indices.size() % 3) == 0);
            if (settings.calculate_normals || mesh.normals.empty())
            {
                recalculate_normals(mesh);
            }

            process_normals(mesh, settings.smoothing_angle);

            if (!mesh.uv_sets.empty())
            {
                process_uvs(mesh);
            }

            process_vertices_static(mesh);
        }

        u64 get_mesh_size(const mesh& m)
        {
            const u64 num_vertices { (m.vertices.size()) };
            const u64 vertex_buffer_size
            {
                sizeof(packed_vertex::vertex_static) * num_vertices
            };

            const u64 index_size { num_vertices < (1 << 16) ? sizeof(u16) : sizeof(u32) };
            const u64 index_buffer_size { index_size * m.indices.size() };

            constexpr u64 su32 { sizeof(u32) };

            const u64 size {
                su32 +               //mesh name length
                m.name.size() +      //room for mesh name string
                su32 +               //lod id
                su32 +               //vertex size
                su32 +               //number of vertices
                su32 +               //indices size(16 bits or 32 bits)
                su32 +               //number of indices
                sizeof(f32) +        //LOD threshold
                vertex_buffer_size + //room for vertices
                index_buffer_size    //room for indices
            };

            return size;
        }

        u64 get_scene_size(const scene& scene)
        {
            constexpr u64 su32 { sizeof(u32) };

            u64 size {
                su32 +              //name length
                scene.name.size() + //room for scene name string
                su32                //lod count
            };

            for (auto& lod : scene.lod_groups)
            {
                u64 lod_size {
                    su32 +            // lod name length
                    lod.name.size() + // room for lod name string
                    su32              // mesh count
                };

                for (auto& mesh : lod.meshes)
                {
                    lod_size += get_mesh_size(mesh);
                }

                size += lod_size;
            }
            return size;
        }

        void pack_mesh_data(const mesh& mesh, u8* const buffer, u64& at)
        {
            constexpr u64 su32 { sizeof(u32) };
            u32           s { 0 };

            //copy mesh name length
            s = static_cast<u32>(mesh.name.size());
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy mesh name
            memcpy(&buffer[at], mesh.name.c_str(), s);
            at += s;

            //copy lod id
            s = mesh.lod_id;
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy vertex size
            constexpr u32 vertex_size { sizeof(packed_vertex::vertex_static) };
            s = vertex_size;
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy number of vertices
            const u32 num_vertices { static_cast<u32>(mesh.vertices.size()) };
            s = num_vertices;
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy indices size
            const u32 index_size { static_cast<u32>(num_vertices < 1 << 16 ? sizeof(u16) : sizeof(u32)) };
            s = index_size;
            memcpy(&buffer[at], &s, su32);
            at += su32;

            // copy number of indices
            const u32 num_indices { static_cast<u32>(mesh.indices.size()) };
            s = num_indices;
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy LOD threshold
            memcpy(&buffer[at], &mesh.lod_threshold, sizeof(f32));
            at += sizeof(f32);

            //copy vertices
            s = num_vertices * vertex_size;
            memcpy(&buffer[at], mesh.packed_vertices_statics.data(), s);
            at += s;

            //copy indices
            s = num_indices * index_size;

            void* data { (void*)mesh.indices.data() };

            util::vector<u16> indices_16;
            if (index_size == sizeof(u16))
            {
                indices_16.resize(num_indices);
                for (u32 i { 0 }; i < num_indices; ++i)
                {
                    indices_16[i] = static_cast<u16>(mesh.indices[i]);
                }
                data = (void*)indices_16.data();
            }

            memcpy(&buffer[at], data, s);
            at += s;
        }
    }

    void process_scene(scene& scene, const geometry_import_settings& settings)
    {
        for (auto& lod : scene.lod_groups)
        {
            for (auto& mesh : lod.meshes)
            {
                process_vertices(mesh, settings);
            }
        }
    }

    void pack_data(scene& scene, scene_data& data)
    {
        constexpr u64 su32 { sizeof(u32) };

        const u64 scene_size { get_scene_size(scene) };

        data.buffer_size = static_cast<u32>(scene_size);
        data.buffer      = static_cast<u8*>(CoTaskMemAlloc(data.buffer_size));

        assert(data.buffer);

        u8* const buffer { data.buffer };
        u64       at { 0 };
        u32       s { 0 };

        //copy scene name length
        s = static_cast<u32>(scene.name.size());
        memcpy(&buffer[at], &s, su32);
        at += su32;

        //copy scene name
        memcpy(&buffer[at], scene.name.c_str(), s);
        at += s;

        //copy lod count
        s = static_cast<u32>(scene.lod_groups.size());
        memcpy(&buffer[at], &s, su32);
        at += su32;

        for (auto& lod : scene.lod_groups)
        {
            //copy lod name length
            s = static_cast<u32>(lod.name.size());
            memcpy(&buffer[at], &s, su32);
            at += su32;

            //copy lod name
            memcpy(&buffer[at], lod.name.c_str(), s);
            at += s;

            //copy mesh count
            s = static_cast<u32>(lod.meshes.size());
            memcpy(&buffer[at], &s, su32);
            at += su32;

            for (auto& mesh : lod.meshes)
            {
                pack_mesh_data(mesh, buffer, at);
            }
        }
        assert(at == data.buffer_size);
    }
}
