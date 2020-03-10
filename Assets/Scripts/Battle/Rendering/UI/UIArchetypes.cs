using Unity.Entities;

namespace Reactics.UI
{
    public static class UIArchetypes
    {
        public static readonly EntityArchetype DirtyUIElement;

        public static readonly EntityArchetype CleanUIElement;



        static UIArchetypes()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            DirtyUIElement = manager.CreateArchetype(typeof(LocalToScreen), typeof(Reactics.UI.UIElement), typeof(EntityGuid));
            CleanUIElement = manager.CreateArchetype(typeof(LocalToScreen), typeof(Reactics.UI.UIElement), typeof(UIClean), typeof(EntityGuid));
        }

    }
}