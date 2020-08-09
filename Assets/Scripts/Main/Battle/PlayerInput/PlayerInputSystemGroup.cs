using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Reactics.Battle
{

    /// <summary>
    /// System Group for storing Systems for determining player input. The Processing of player inputs do not go here, but go in the Player Input Processing Group. 
    /// 
    /// </summary>
    [UpdateInGroup(typeof(BattleSystemGroup))]
    public class PlayerInputSystemGroup : ComponentSystemGroup
    {
    }
    //Clean up inputs to a readable format for the PlayerInputProcessorSystem.
    [UpdateInGroup(typeof(PlayerInputSystemGroup))]
    public class PlayerInputInterpreterSystem : ComponentSystem
    {
        protected override void OnUpdate() 
        {
            InputSystem.Update();
            if (BattlePlayer.instance == null)
                return;
            
            //TODO: Check if just setting the battleplayer.instance saves time (does calling battleplayer.instance like this result in a call every time..? probably.)
            Controls input = BattlePlayer.instance.input;
            var playerInput = BattlePlayer.instance.playerInput;
            float screenEdgeLength = 40f;
            ControlSchemes controlScheme = BattlePlayer.instance.GetControlScheme();
            //var actionMap = BattlePlayer.instance.GetActionMap(); //maybe unnecessary.

            //Check action map so we know which inputs we're going to be dealing with
            InputData inputData = GetSingleton<InputData>();
            CursorData cursorData = GetSingleton<CursorData>();

            Entities.ForEach((ref ControlSchemeData controlSchemeData) => //maybe this is really the way to go...
            {
                //CAN WE DO THE CHECK FOR CHANGES THING HERE?
                controlSchemeData.currentControlScheme = controlScheme;
            });

            //Receive battle controls inputs only.
            if (inputData.currentActionMap == ActionMaps.BattleControls)
            {
                //Player inputs
                //input.BattleControls.Hover.actionMap.name; Here's a way to check the action map, apparently~
                bool cancelTileInput = input.BattleControls.CancelTile.triggered;
                Vector2 hoverInput = input.BattleControls.Hover.ReadValue<Vector2>();
                Vector2 tileMovementInput = input.BattleControls.TileMovement.ReadValue<Vector2>();
                float cameraZoomInput = input.BattleControls.CameraZoom.ReadValue<float>();
                Vector2 rotationInput = input.BattleControls.Camera.ReadValue<Vector2>();
                bool selectTileInput = cancelTileInput ? false : input.BattleControls.SelectTile.triggered;
                Ray mousePositionRayCast = new Ray();

                //Clean up camera panning inputs
                //These inputs are converted to simpler values to pan the camera certain directions.
                float2 cleanedHoverInput = new float2(0, 0);
                if (playerInput.currentControlScheme == "Keyboard + Mouse")
                {
                    //Get the world coordinates of the mouse based on a raycast
                    mousePositionRayCast = BattlePlayer.instance.GetMouseCursorWorldCoordinates(hoverInput);
                    
                    //If the mouse is close enough to the edge of the screen, pan appropriately.
                    if (hoverInput.y >= Screen.height - screenEdgeLength)
                        cleanedHoverInput.y = 1f;
                    else if (hoverInput.y <= screenEdgeLength)
                        cleanedHoverInput.y = -1f;
                    if (hoverInput.x >= Screen.width - screenEdgeLength)
                        cleanedHoverInput.x = 1f;
                    else if (hoverInput.x <= screenEdgeLength)
                        cleanedHoverInput.x = -1f;

                    //raycast stuff. maybe this goes with the othe rthing? idk. for now just make it raycast, yaeh..?
                    cursorData.rayOrigin = mousePositionRayCast.origin;
                    cursorData.rayDirection = mousePositionRayCast.direction;
                    SetSingleton<CursorData>(cursorData);
                }
                else if (playerInput.currentControlScheme == "Gamepad")
                {
                    //Set the pan direction to the control stick direction
                    //Note it doesn't currentlyg et set to 1. not a huge deal but should probably clean tha tup.
                    cleanedHoverInput = hoverInput; //just a thought, maybe this doesn't work because one's a float2 and one's a vector2. just thinking.
                }

                //Clean up tile movement inputs (Currently unnecessary. Probably always unnecessary.)
                //These move the camera from one tile to the next.

                //Clean up camera zoom inputs (Currently unnecessary. May need to in the future with camera zoom speed. Most likely scenario, actually.)
                //These zoom the camera in and out.

                //Clean up camera rotation inputs (Currently unnecessary. Probably always unnecessary.)
                //These pivot the camera around the lookat point.

                //Clean up select tile inputs (Currently not tested for mouse, could be unnecessary, most likely isn't.)
                //These select tiles, if applicable.
                //Maybe with get singleton we can meme it up and pass it to the job as readonly apparently.

                //Send Battle Controls inputs.
                inputData.pan = cleanedHoverInput;
                inputData.tileMovement = tileMovementInput;
                inputData.zoom = cameraZoomInput;
                inputData.rotation = rotationInput;
                inputData.select = selectTileInput;
                inputData.cancel = cancelTileInput;
                SetSingleton<InputData>(inputData);
            }
            //Receive command controls inputs only.
            else if (inputData.currentActionMap == ActionMaps.CommandControls)
            {
                bool cancelActionInput = input.CommandControls.CancelAction.triggered;
                bool selectActionInput = cancelActionInput ? false : input.CommandControls.SelectAction.triggered;
                bool menuMovementInput = cancelActionInput ? false : input.CommandControls.MenuMovement.triggered;
                Vector2 menuMovementDirection = input.CommandControls.MenuMovement.ReadValue<Vector2>();

                if (!menuMovementInput)
                    menuMovementDirection = new float2(0,0);

                //Set singleton here so it sets all the camera values to defaults.
                SetSingleton<InputData>(new InputData {
                    menuMovement = menuMovementInput,
                    menuMovementDirection = menuMovementDirection,
                    select = selectActionInput,
                    cancel = cancelActionInput,
                    currentActionMap = ActionMaps.CommandControls,
                    menuOption = inputData.menuOption
                });
            }
        }
    }
}