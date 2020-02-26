//this will do the thing that the top of player inputs does, eventually.
/*
class RandomStartingPositionSystem : JobComponentSystem {
    struct PositionGroup {
        public int Length;
        public ComponentDataArray<Position> Positions;
        public ComponentDataArray<InitWithRandomPosition> RequiredInit; // just added to the group as filter
        public EntityArray Entities;
    }
 
    [Inject] private PositionGroup m_Group;
 
    struct SetRandomPositionJob : IJobParallelFor {
        public ComponentDataArray<Position> positions;
        [ReadOnly]public EntityArray entities;
        public NativeArray<float3> randomPositions;
        [NativeDisableParallelForRestriction]public EntityCommandBuffer commandBuffer;
 
        public void Execute(int index) {
            positions[index] = new Position { Value = randomPositions[index] };
            // Removes the InitWithRandomPosition at the end of frame
            commandBuffer.RemoveComponent<InitWithRandomPosition>(entities[index]);
        }
    }
 
    [Inject] EndFrameBarrier endFrameBarrier;
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float spreader = 5;
        NativeArray<float3> randomPositions = new NativeArray<float3>(m_Group.Length, Allocator.Temp);
        for (int i = 0; i < m_Group.Length; i++) {
            randomPositions[i] = new float3(Random.value * spreader, Random.value * spreader, Random.value * spreader);
        }
 
        var setRandomPositionJob = new SetRandomPositionJob {
            positions = m_Group.Positions,
            entities = m_Group.Entities,
            randomPositions = randomPositions,
            commandBuffer = endFrameBarrier.CreateCommandBuffer()
        };
        var setRandomPositionJobFence = setRandomPositionJob.Schedule(m_Group.Length, 1, inputDeps);
        setRandomPositionJobFence.Complete();
        randomPositions.Dispose();
        return inputDeps;
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(PlayerInputSystem))]
public class InitializationSystem : ComponentSystem
{
    protected override void OnUpdate() 
    {
        //apparently tilesize isn't really in ecs anywhere but this is probably where I'd put it normally
        Entities.ForEach((Entity entity, ref InitializeTag initializeTag, ref CursorData cursorTag) =>
        {
            cursorTag.rayMagnitude = 10000f;
            PostUpdateCommands.RemoveComponent(entity, typeof(InitializeTag));
        });

        Entities.ForEach((Entity entity, ref InitializeTag initializeTag, ref Translation trans, ref CameraMovementData moveData) =>
        {
            moveData.cameraLookAtPoint = new float3(0, 0, 0); //thsi is the origin, later it will be calculated or w/e.
            trans.Value = math.normalize(trans.Value) * moveData.offsetValue;
            moveData.zoomMagnitude = 1f;
            moveData.lowerZoomLimit = 0.1f;
            moveData.upperZoomLimit = 2.0f;
            PostUpdateCommands.RemoveComponent(entity, typeof(InitializeTag));
        });
    }
}