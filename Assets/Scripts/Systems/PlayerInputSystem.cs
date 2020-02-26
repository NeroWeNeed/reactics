using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Reactics.Battle;

//always synchronize? not sure if necessary on component system
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class PlayerInputSystem : JobComponentSystem
{
    /*[SerializeField] figure out later...
    private Camera battleCamera;*/
    private float screenEdgeLength = 40f;
    private Vector2 hoverInput;
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        //theoretically... this runs before all the other systems, yes?
        //moving the mouse makes this struggle. maybe that's normal since it's reading so many inputs? unsure.
        InputSystem.Update(); //just this line makes this take way longer (it's 0.3ms but like, that's way more than the normal 0.02 it was before?)

        if (BattlePlayer.instance == null)
            return default;

        //maybe have a return here if using different action map (non-battle controls, like menus or something, or maybe whatever happens if you select a unit, etc)
        var input = BattlePlayer.instance.input;
        var playerInput = BattlePlayer.instance.playerInput;
        var controlScheme = BattlePlayer.instance.GetControlScheme();
        Ray mousePositionRayCast = new Ray();

        //Get any inputs 
        hoverInput = input.BattleControls.Hover.ReadValue<Vector2>(); //for some reason, gamepad doesn't work here... very strange.
        Vector2 rotationDirection = input.BattleControls.Camera.ReadValue<Vector2>();
        Vector2 gridMovement = input.BattleControls.TileMovement.ReadValue<Vector2>();
        float cameraZoom = input.BattleControls.CameraZoom.ReadValue<float>();

        //find a way to make this not run every frame maybe, otherwise yeah.. waste of processing power
        Entities.ForEach((ref ControlSchemeData controlSchemeData) =>
        {
            controlSchemeData.currentControlScheme = controlScheme;
        }).Run();

        //In this case, we pan if the mouse is at the edge of the screen
        float2 panMovement = new float2(0, 0);
        if (playerInput.currentControlScheme == "Keyboard + Mouse")
        {
            mousePositionRayCast = BattlePlayer.instance.GetMouseCursorWorldCoordinates(hoverInput);
            
            if (hoverInput.y >= Screen.height - screenEdgeLength)
                panMovement.y = 1f;
            else if (hoverInput.y <= screenEdgeLength)
                panMovement.y = -1f;
            if (hoverInput.x >= Screen.width - screenEdgeLength)
                panMovement.x = 1f;
            else if (hoverInput.x <= screenEdgeLength)
                panMovement.x = -1f;
        }
        else if (playerInput.currentControlScheme == "Gamepad")
        {
            panMovement = hoverInput;
        }

        //If an input is detected that would move the camera, set those inputs on the respective data components
        //Note: This if statement seems to *really* speed up this system, maybe since it's not running the job all the time anymore. makes sense.
        //okay maybe actually ti doesn't?? maybe it acutally doesn't do that at all actually
        if (rotationDirection.magnitude > 0 || hoverInput.magnitude > 0 || cameraZoom > 0.1f || cameraZoom < 0.1f) 
        {
        Entities.ForEach((ref CameraMovementData moveData, ref CameraRotationData rotData) => //Speed is set in the editor
        {
            //don't do camera stuff if it's actively rotating or it gets really mad
            if (!rotData.rotating)
            {
                moveData.panMovementDirection = panMovement;
                moveData.gridMovementDirection = gridMovement;
                moveData.zoomDirectionAndStrength = cameraZoom;
                rotData.rotationDirection = rotationDirection;
                //If we're done moving then center the camera to the tile it's currently in
                /*if (!moveData.moving)
                {
                    if (currentControlScheme.CompareTo(gamepadControlScheme) == 0) //assuming 0 is true
                    {
                        //add tag...?
                    }
                }*/
            }
            //well, anyway. how do we like... raycast or whatever?
            //This value is the current mouse position.
            //also it's become apparent that we need to have systems for like. each thing.
            //isn't checking for input every frame stupid? maybe not if that's what the input system already does...?
            //also we still aren't entirely... sure what the control mode is from the inputs alone?
        }).Run();
        }
        if (playerInput.currentControlScheme == "Gamepad")
        {
            Entities.ForEach((ref CameraMovementData moveData) => 
            {
                //tile snap...?
            }).Run();
        }
        else
        {
            Entities.ForEach((ref CursorData cursorTag) =>
            {
                cursorTag.rayOrigin = mousePositionRayCast.origin;
                cursorTag.rayDirection = mousePositionRayCast.direction;
            }).Run();
        }
        return default;
    }
}