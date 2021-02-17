using Unity.Collections;
using Unity.Entities;

namespace NeroWeNeed.UIDots {
    public class UISchemaInitSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithNone<UICompiledSchemaData>().ForEach((Entity entity, in UISchema schema) =>
            {
                EntityManager.AddComponentData(entity, new UICompiledSchemaData { value = schema.Compile(Allocator.Persistent) });
            }).WithoutBurst().WithStructuralChanges().Run();
        }
        protected override void OnDestroy() {
            Entities.ForEach((Entity entity, in UICompiledSchemaData schema) =>
            {
                schema.value.Dispose();
                EntityManager.RemoveComponent<UICompiledSchemaData>(entity);
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}