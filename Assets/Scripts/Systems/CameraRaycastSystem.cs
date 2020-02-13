/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Reactics.Battle;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CameraRotationSystem))]
public class CameraRaycastSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        //ComponentDataFromEntity<CameraMovementData> cameraData = GetComponentDataFromEntity<CameraMovementData>(true);
        Entities.ForEach((ref Translation trans, CameraMovementData moveData) =>
        {
           Camera.main.ScreenPointToRay() //dumb obviously
        }).Run();
        return default;
    }
}*/