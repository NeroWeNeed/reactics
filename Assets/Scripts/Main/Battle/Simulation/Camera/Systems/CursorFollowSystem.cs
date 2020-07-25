using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Reactics.Battle;
using Unity.Physics;

//always synchronize? not sure if necessary on component system
[UpdateInGroup(typeof(RenderingSystemGroup))]
[UpdateAfter(typeof(CameraRotationSystem))]
public class CursorFollowSystem : SystemBase
{
    
    protected override void OnUpdate() 
    {
        ComponentDataFromEntity<CameraMovementData> cameraData = GetComponentDataFromEntity<CameraMovementData>(true);
        //Apparently doing this is just allowed.
        var physicsWorldSystem = this.World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Entities.WithReadOnly(cameraData).ForEach((ref Translation trans, in CursorData cursorData, in ControlSchemeData controlSchemeData) =>
        {
            if (!cameraData.HasComponent(cursorData.cameraEntity))
                return;
            if (controlSchemeData.currentControlScheme == ControlSchemes.Gamepad)
            {
                trans.Value = cameraData[cursorData.cameraEntity].cameraLookAtPoint;
            }
            else if (controlSchemeData.currentControlScheme == ControlSchemes.KeyboardAndMouse)
            {
                //Since we have some ray stuff from the camera we don't need a Ray object
                RaycastInput input = new RaycastInput
                {
                    Start = cursorData.rayOrigin, //Start point
                    End = cursorData.rayOrigin + (cursorData.rayDirection * cursorData.rayMagnitude), //End point
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u, //allow everything to collide with this ray
                        CollidesWith = ~0u, //make this ray try to collide with everything
                        GroupIndex = 0 //idk that's what everyone else did
                    }
                };
                RaycastHit hit = new RaycastHit();
                bool haveHit = collisionWorld.CastRay(input, out hit);
                if (haveHit)
                {
                    trans.Value = hit.Position;
                }
                //UnityEngine.Debug.DrawRay(cursorData.rayOrigin, cursorData.rayDirection * cursorData.rayMagnitude, UnityEngine.Color.yellow);
            }
        }).Run();
    }
}