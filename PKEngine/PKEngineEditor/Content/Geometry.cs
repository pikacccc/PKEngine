using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using PKEngineEditor.Common;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.Content
{
    public enum PrimitiveMeshType
    {
        Plane,
        Cube,
        UvSphere,
        IcoSphere,
        Cylinder,
        Capsule,

        Count
    }

    public class Mesh : ViewModelBase
    {
        private int _vertexSize;

        public int VertexSize
        {
            get => _vertexSize;
            set
            {
                if (value != _vertexSize)
                {
                    _vertexSize = value;
                    OnPropertyChanged(nameof(VertexSize));
                }
            }
        }

        private int _vertexCount;

        public int VertexCount
        {
            get => _vertexCount;
            set
            {
                if (value != _vertexCount)
                {
                    _vertexCount = value;
                    OnPropertyChanged(nameof(VertexCount));
                }
            }
        }

        private int _indexSize;

        public int IndexSize
        {
            get => _indexSize;
            set
            {
                if (value != _indexSize)
                {
                    _indexSize = value;
                    OnPropertyChanged(nameof(IndexSize));
                }
            }
        }

        private int _indexCount;

        public int IndexCount
        {
            get => _indexCount;
            set
            {
                if (value != _indexCount)
                {
                    _indexCount = value;
                    OnPropertyChanged(nameof(IndexCount));
                }
            }
        }

        public byte[] Vertices { get; set; } = null!;
        public byte[] Indices  { get; set; } = null!;
    }

    public class MeshLOD : ViewModelBase
    {
        private string _name = null!;

        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private float _lodThreshold;

        public float LodThreshold
        {
            get => _lodThreshold;
            set
            {
                if (value != _lodThreshold)
                {
                    _lodThreshold = value;
                    OnPropertyChanged(nameof(LodThreshold));
                }
            }
        }

        public ObservableCollection<Mesh> Meshes { get; } = new();
    }

    public class LODGroup : ViewModelBase
    {
        private string _name = null!;

        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ObservableCollection<MeshLOD> LODs { get; } = new();
    }

    public class Geometry : Asset
    {
        private readonly List<LODGroup> _lodGroups = new();

        public LODGroup GetLODGroup(int lodGroupIndex = 0)
        {
            Debug.Assert(lodGroupIndex >= 0 && lodGroupIndex < _lodGroups.Count);
            return (_lodGroups.Any() ? _lodGroups[lodGroupIndex] : null)!;
        }

        public Geometry() : base(AssetType.Mesh)
        {
        }

        private static void ReadMeshes(BinaryReader reader, ref List<int> lodIds, ref List<MeshLOD> lodList)
        {
            var    s = reader.ReadInt32();
            string meshName;
            if (s > 0)
            {
                var nameBytes = reader.ReadBytes(s);
                meshName = Encoding.UTF8.GetString(nameBytes);
            }
            else
            {
                meshName = $"mesh_{ContentHelper.GetRandomString()}";
            }

            var mesh = new Mesh();

            var lodId = reader.ReadInt32();
            mesh.VertexSize  = reader.ReadInt32();
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize   = reader.ReadInt32();
            mesh.IndexCount  = reader.ReadInt32();
            var lodThreshold = reader.ReadSingle();

            var vertextBufferSize = mesh.VertexCount * mesh.VertexSize;
            var indexBufferSize   = mesh.IndexCount  * mesh.IndexSize;

            mesh.Vertices = reader.ReadBytes(vertextBufferSize);
            mesh.Indices  = reader.ReadBytes(indexBufferSize);

            MeshLOD lod;
            if (ID.IsValid(lodId) && lodIds.Contains(lodId))
            {
                lod = lodList[lodIds.IndexOf(lodId)];
                Debug.Assert(lod != null);
            }
            else
            {
                lodIds.Add(lodId);
                lod = new MeshLOD() { Name = meshName, LodThreshold = lodThreshold };
                lodList.Add(lod);
            }

            lod.Meshes.Add(mesh);
        }

        private static List<MeshLOD> ReadMeshLODs(int numMeshes, BinaryReader reader)
        {
            var lodIds  = new List<int>();
            var lodList = new List<MeshLOD>();
            for (int i = 0; i < numMeshes; i++)
            {
                ReadMeshes(reader, ref lodIds, ref lodList);
            }

            return lodList;
        }

        internal void FromRawData(ref byte[] data)
        {
            Debug.Assert(data?.Length > 0);
            _lodGroups.Clear();

            using var reader = new BinaryReader(new MemoryStream(data));

            //skip scene name string
            var s = reader.ReadInt32();
            reader.BaseStream.Position += s;

            //get number of lods
            var numLODGroups = reader.ReadInt32();
            Debug.Assert(numLODGroups > 0);

            for (int i = 0; i < numLODGroups; i++)
            {
                s = reader.ReadInt32();
                string lodGroupName;
                if (s > 0)
                {
                    var nameBytes = reader.ReadBytes(s);
                    lodGroupName = Encoding.UTF8.GetString(nameBytes);
                }
                else
                {
                    lodGroupName = $"lod_{ContentHelper.GetRandomString()}";
                }

                var numMeshes = reader.ReadInt32();
                Debug.Assert(numMeshes > 0);
                var lods = ReadMeshLODs(numMeshes, reader);

                var lodGroup = new LODGroup() { Name = lodGroupName };
                lods.ForEach(l => lodGroup.LODs.Add(l));
            }
        }
    }
}