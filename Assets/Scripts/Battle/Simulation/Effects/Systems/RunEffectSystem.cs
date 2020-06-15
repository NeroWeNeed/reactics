using Unity.Entities;

namespace Reactics.Battle
{
    public class RunEffectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, in EffectReference effectReference) =>
            {

            }).WithoutBurst().Run();
        }

        public struct RunEffectJob<T> : IJobChunk where T : unmanaged
        {

            public ArchetypeChunkEntityType entities;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}