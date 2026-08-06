using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using PKEngineEditor.Content;
using PKEngineEditor.ContentToolsAPIStructs;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.ContentToolsAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public class GeometryImportSettings
    {
        public float SmoothingAngle         = 178f;
        public byte  CalculateNormals       = 0;
        public byte  CalculateTangents      = 1;
        public byte  ReverseHandedness      = 0;
        public byte  ImportEmbeddedTextures = 1;
        public byte  ImportAnimations       = 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SceneData : IDisposable
    {
        public IntPtr                 Data;
        public int                    DataSize;
        public GeometryImportSettings ImportSettings = new();

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(Data);
            GC.SuppressFinalize(this);
        }

        ~SceneData()
        {
            Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PrimitiveInitInfo
    {
        public PrimitiveMeshType Type;
        public int               SegmentX = 1;
        public int               SegmentY = 1;
        public int               SegmentZ = 1;
        public Vector3           Size     = new(1, 1, 1);
        public int               Lod      = 0;
    }
}

namespace PKEngineEditor.DllWrappers
{
    static class ContentToolsAPI
    {
        private const string ToolsDll = "ContentTools.dll";

        [DllImport(ToolsDll)]
        private static extern void CreatePrimitiveMesh([In, Out] SceneData data, PrimitiveInitInfo info);

        public static void CreatePrimitiveMesh(PrimitiveInitInfo info, out Geometry geometry)
        {
            Debug.Assert(geometry != null, "Geometry cannot be null");
            using var sceneData = new SceneData();
            try
            {
                CreatePrimitiveMesh(sceneData, info);
                Debug.Assert(sceneData.Data != IntPtr.Zero && sceneData.DataSize > 0);
                var data = new byte[sceneData.DataSize];
                Marshal.Copy(sceneData.Data, data, 0, sceneData.DataSize);
                geometry = new Geometry();
                geometry.FromRawData(ref data);
            }
            catch (Exception)
            {
                Logger.Log(MessageType.Error, $"failed to create {info.Type} primitive mesh.");
                throw;
            }
        }
    }
}