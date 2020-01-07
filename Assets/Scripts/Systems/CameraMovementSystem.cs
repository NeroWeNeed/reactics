using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public class CameraMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        Entities.ForEach((ref CameraMovementData data) => 
        {
            //InputAction.CallbackContext probably doesn't work~
        }).Run();
        return default;
    }
}
