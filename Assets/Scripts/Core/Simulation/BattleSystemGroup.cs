using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Reactics.Core.Battle {
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    public class BattleSystemGroup : ComponentSystemGroup {

        public static void InitializeGroup(World world) {

        }
    }
}