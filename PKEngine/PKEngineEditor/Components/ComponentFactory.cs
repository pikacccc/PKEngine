using System.Diagnostics;

namespace PKEngineEditor.Components
{
    public enum ComponentType
    {
        Transform = 0,
        Script
    }

    static class ComponentFactory
    {
        private static readonly Func<GameEntity, object, Component>[] Functions =
        [
            (entity, data) => new Transform(entity),
            (entity, data) => new Script(entity) { Name = (string)data }
        ];

        public static Func<GameEntity, object, Component> GetCreationFunction(ComponentType componentType)
        {
            Debug.Assert((int)componentType < Functions.Length);
            return Functions[(int)componentType];
        }

        public static ComponentType ToEnumType(this Component component)
        {
            return component switch
            {
                Transform => ComponentType.Transform,
                Script => ComponentType.Script,
                _ => throw new ArgumentException("Unknown component type")
            };
        }
    }
}