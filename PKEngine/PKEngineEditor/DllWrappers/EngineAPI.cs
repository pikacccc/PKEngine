using PKEngineEditor.Components;
using PKEngineEditor.EngineAPIStructs;
using System.Numerics;
using System.Runtime.InteropServices;
using PKEngineEditor.GameProject;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new(1, 1, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponent
    {
        public IntPtr ScriptCreator;
    }

    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public readonly TransformComponent Transform = new();
        public readonly ScriptComponent    Script    = new();
    }
}

namespace PKEngineEditor.DllWrappers
{
    internal static class EngineAPI
    {
        private const string EngineDll = "EngineDll.dll";

        [DllImport(EngineDll, CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string dllPath);

        [DllImport(EngineDll)]
        public static extern int UnloadGameCodeDll();

        [DllImport(EngineDll)]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(EngineDll)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        [DllImport(EngineDll)]
        public static extern int CreateRenderSurface(IntPtr host, int width, int height);

        [DllImport(EngineDll)]
        public static extern void RemoveRenderSurface(int id);

        [DllImport(EngineDll)]
        public static extern IntPtr GetRenderHandle(int id);

        [DllImport(EngineDll)]
        public static extern void ResizeRenderSurface(int id);

        internal static class EntityAPI
        {
            [DllImport(EngineDll)]
            private static extern int CreateGameEntity(GameEntityDescriptor desc);

            public static int CreateGameEntity(GameEntity entity)
            {
                GameEntityDescriptor desc = new GameEntityDescriptor();

                // transform component
                {
                    var c = entity.GetComponent<Transform>();
                    desc.Transform.Position = c!.Position;
                    desc.Transform.Rotation = c.Rotation;
                    desc.Transform.Scale    = c.Scale;
                }

                // script component
                {
                    var c = entity.GetComponent<Script>();
                    if (c != null && Project.CurProject != null)
                    {
                        if (Project.CurProject.AvailableScripts.Contains(c.Name))
                        {
                            desc.Script.ScriptCreator = GetScriptCreator(c.Name);
                        }
                        else
                        {
                            Logger.Log(MessageType.Error,
                                       $"Unable to find script with name {c.Name}. Game entity will be created without script component!");
                        }
                    }
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