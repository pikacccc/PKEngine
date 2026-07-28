using PKEngineEditor.Components;
using PKEngineEditor.EngineAPIStructs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PKEngineEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new Vector3(1, 1, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new TransformComponent();
    }
}

namespace PKEngineEditor.DllWrappers
{
    static class EngineAPI
    {
        private const string EngineDll = "EngineDll.dll";

        [DllImport(EngineDll,CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string dllPath);
        
        [DllImport(EngineDll)]
        public static extern int UnloadGameCodeDll();
        
        internal static class EntityAPI
        {
            [DllImport(EngineDll)]
            private static extern int CreateGameEntity(GameEntityDescriptor desc);

            public static int CreateGameEntity(GameEntity entity)
            {
                GameEntityDescriptor desc = new GameEntityDescriptor();

                {
                    var c = entity.GetComponent<Transform>();
                    desc.Transform.Position = c.Position;
                    desc.Transform.Rotation = c.Rotation;
                    desc.Transform.Scale = c.Scale;
                }
                return CreateGameEntity(desc);
            }

            [DllImport(EngineDll)]
            private static extern void RemoveGameEntity(int id);

            public static void RemoveGameEntity(GameEntity entity)
            {
                RemoveGameEntity(entity.EngineId);
            }
        }
    }
}