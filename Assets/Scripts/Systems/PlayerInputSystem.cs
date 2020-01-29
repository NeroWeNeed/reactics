using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

//always synchronize? not sure if necessary on component system
[AlwaysSynchronizeSystem]
public class PlayerInputSystem : ComponentSystem
{
    private float screenEdgeLength = 40f;
    private static bool initializer = false;
    protected override void OnUpdate() 
    {
        //TODO: Have an initialization system instead of doing stuff here. that's gonna require some looking up tho.
        if (!initializer)
        {
            Entities.ForEach((ref Translation trans, ref CameraMovementData data) =>
            {
                data.cameraLookAtPoint = new float3(0, 0, 0); //thsi is the origin, later it will be calculated or w/e.
                data.zoomMagnitude = 1f;
                data.lowerZoomLimit = 0.1f;
                data.upperZoomLimit = 2.0f;
                trans.Value.y = 100f; //this is just for now to do camera testing shenanigans...
            });
            initializer = true; //this is really bad pls don't do this
        }
        
        
        //theoretically... this runs before all the other systems, yes?
        //moving the mouse makes this struggle. maybe that's normal since it's reading so many inputs? unsure.
        InputSystem.Update(); //just this line makes this take way longer (it's 0.3ms but like, that's way more than the normal 0.02 it was before?)
        var input = BattlePlayer.instance.input;

        if (BattlePlayer.instance == null)
            return;
        
        float2 mousePosition = input.BattleControls.Hover.ReadValue<Vector2>();
        float2 movementDirection = new float2(0, 0);

        if (mousePosition.y >= Screen.height - screenEdgeLength)
            movementDirection.y = 1f;
        else if (mousePosition.y <= screenEdgeLength)
            movementDirection.y = -1f;
        if (mousePosition.x >= Screen.width - screenEdgeLength)
            movementDirection.x = 1f;
        else if (mousePosition.x <= screenEdgeLength)
            movementDirection.x = -1f;
        
        Entities.ForEach((ref CameraMovementData moveData) => //Speed is set in the editor
        {
            moveData.movementDirection = movementDirection;
            moveData.zoomDirectionAndStrength = input.BattleControls.CameraZoom.ReadValue<float>();
            //well, anyway. how do we like... raycast or whatever?
            //This value is the current mouse position.
            //also it's become apparent that we need to have systems for like. each thing.
            //isn't checking for input every frame stupid? maybe not if that's what the input system already does...?
            //also we still aren't entirely... sure what the control mode is from the inputs alone?
        });

        Entities.ForEach((ref CameraRotationData rotData) =>
        {
            //to figure out what control scheme we're using we could theoretically not ahve them share action maps 
            if (!rotData.rotating)
            {
                rotData.rotationDirection = input.BattleControls.Camera.ReadValue<Vector2>();
            }
        });
    }
}