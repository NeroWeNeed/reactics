using System;
using System.Collections;
using System.Linq;
using Reactics.Core.Battle;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
namespace Reactics.Core.Input {
    //TODO: Compact into a system. put playerinput data into a component. You can reference the camere with the CameraReference component, and the type with the CameraTag component.

    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
    [RequireComponent(typeof(GameObjectEntity))]
    public class InputHandler : MonoBehaviour, IConvertGameObjectToEntity {

        private static InputHandler _instance;
        public static InputHandler instance => _instance;
        private World world;

        [HideInInspector]
        private new UnityEngine.Camera camera;

        public Controls input;
        [HideInInspector]
        public UnityEngine.InputSystem.PlayerInput playerInput;
        [HideInInspector, SerializeField]
        private bool needsUpdateControls = false;
        public bool NeedsUpdateControls { get => needsUpdateControls; internal set => needsUpdateControls = value; }
        private void Awake() {
            _instance = this;
            input = new Controls();
            playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();

        }
        private void OnEnable() {
            //input.Enable();
        }
        private void OnDisable() {
            //input.Disable();
        }
        public ControlSchemes GetControlScheme() {
            if (playerInput.currentControlScheme == "Gamepad")
                return ControlSchemes.Gamepad;
            return ControlSchemes.KeyboardAndMouse;
        }

        public Ray GetMouseCursorWorldCoordinates(Vector2 mouseCoordinates) {
            return camera.ScreenPointToRay(mouseCoordinates);
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null) {
                dstManager.AddSharedComponentData(entity, new InputHandlerData { value = this.gameObject });

                dstManager.AddSharedComponentData(entity, new InputContext { actionMapName = playerInput.defaultActionMap, controlSchemeName = playerInput.defaultControlScheme });
                dstManager.AddComponentData(entity, new InputHandlerStateData { value = InputHandlerState.Clean });
                dstManager.AddComponentData(entity, new PlayerIndexData { value = playerInput.playerIndex });
                dstManager.AddComponentObject(entity, playerInput);
                GameObjectEntity.AddToEntity(dstManager, this.gameObject, entity);
            }
            var cameraEntity = conversionSystem.TryGetPrimaryEntity(UnityEngine.Camera.main);
            if (cameraEntity != Entity.Null) {
                dstManager.AddComponentData(entity, new CameraDependent { cameraEntity = cameraEntity });
            }

        }
        /*
   [SerializeField]
   private BattleGridCamera camera;

   //maybe make this a part of the battlegridmanager or something? idk.
   private float screenEdgeLength = 40f;

   //[SerializeField]
   //private BattleGridCameraFocus cameraFocus;

   [SerializeField]
   private Cursor cursor;
   [SerializeField]
   private Reactics.Battle.Map.MapRenderer mapRenderer;

   private BattleGridManager battleGridManager;

   private Controls controls;

   private PlayerInput input;

   private void Start()
   {
       battleGridManager = GetComponentInParent<BattleGridManager>();
       //Debug.Log("REAL WIDTH:" + battleGridManager.RealWidth);
       input = GetComponent<PlayerInput>();
       if (controls == null)
           controls = new Controls();

       if (input.currentControlScheme == "Keyboard + Mouse")
       {
           //cameraFocus.TogglePointer(true);
           cursor.TogglePointer(true);
       }
       else
       {
           //cameraFocus.TogglePointer(false);
           cursor.TogglePointer(false);
       }

       //Stops camera from moving if control scheme set to gamepad (probably a better way to do this?)
       InputUser.onChange += (user, change, device) => {
           if (change == InputUserChange.ControlSchemeChanged)
           {
               if (input.currentControlScheme == "Keyboard + Mouse")
               {
                   //cameraFocus.TogglePointer(true);
                   cursor.TogglePointer(true);
               }
               else
               {
                   //cameraFocus.TogglePointer(false);
                   cursor.TogglePointer(false);
               }
           }
       };
       //actionMap = controls.BattleControls.Get();
   }

   //TODO: Figure out why all these methods get called twice for one input
   public void UpdateCameraOrientation(InputAction.CallbackContext context)
   {
       //TODO: Add center camera button for keyboard/mouse
       Vector2 value = context.ReadValue<Vector2>();
       if (value.x > 0.5)
       {
           camera.PrevHorizontalOrientation();// DOTS experiments here... oh boy.
           //first, get the wait. we konw the input because of the input system so we can just choose the thing it goes to.
           //like we know for a fact what we want to do with this input so the control scheme doesn't erally matter. so that's cool.
           //maybe when we have more modes of control we have to check the current control scheme but we'll get to that, shouldn't be difficult.

           //FIRST: set up some data. which should be a struct. this is moving an object from one place to another. that should be the first thing.
           //MovementData data = 
           //MovementData data = new MovementData{immediate = false, targetPosition = new float3(0f, 0f, 0f), moving=true, speed = 10f};

       }
       else if (value.x < -0.5)
       {
           camera.NextHorizontalOrientation();
       }
       if (value.y > 0.5)
       {
           camera.NextVerticalOrientation();
       }
       else if (value.y < -0.5)
       {
           camera.PrevVerticalOrientation();
       }
   }
   public void MouseHover(InputAction.CallbackContext context)
   {
       Vector2 value = context.ReadValue<Vector2>();
       //Debug.Log(context.ReadValue<Vector2>());
       if (input.currentControlScheme == "Keyboard + Mouse")
       {
           //if ()
           //cameraFocus.SetCurrentMousePosition(value);
           if (value.y >= Screen.height - screenEdgeLength)
               transform.Translate(0, 0, cameraSpeed, Space.Self);
           if (value.y <= screenEdgeLength)
               transform.Translate(0, 0, -cameraSpeed, Space.Self);
           if (value.x >= Screen.width - screenEdgeLength)
               transform.Translate(cameraSpeed, 0, 0, Space.Self);
           if (value.x <= screenEdgeLength)
               transform.Translate(-cameraSpeed, 0, 0, Space.Self);
           cursor.SetCurrentMousePosition(value);
           //Vector2Int? cfwp = mapRenderer.CoordinateFromWorldPoint(value);
           //Debug.Log("MP: " + value);
           //Debug.Log("CFWP: " + cfwp);
           //Debug.Log("TILE: " + mapRenderer.Map[(Vector2Int)cfwp]);
           //Debug.Log("MAPRENDERER: " + mapRenderer.Map.IndexOf();
           //Debug.Log(battleGridManager.Grid.GetTile(value2));
       }
       else if (input.currentControlScheme == "Gamepad")
       {
           //cameraFocus.SetLeftStickStrength(value);
       }

   }

   public void TileMovement(InputAction.CallbackContext context)
   {
       Vector2 value = context.ReadValue<Vector2>();
       Vector2Int? tileArrayPos = null;//mapRenderer.CoordinateFromWorldPoint(cameraFocus.transform.position);
       if (tileArrayPos != null)
       {
           Vector2Int thing = (Vector2Int)tileArrayPos;
           Debug.Log("value: " + value);
           Debug.Log(tileArrayPos);
           //find the tile we're in, then do the 4 way if statement to move one tile to the left/right/up/down (direction tbd)
           //to fix the "holding down the key" thing just have it set a value instead of being constant.
           //ex set some bool "dpad held" and just constantly apply the value in update, with maybe some sort of "cooldown" so it doesn't teleport..."
           Vector3 pos = new Vector3(tileArrayPos.Value.x*mapRenderer.TileSize + mapRenderer.TileSize/2, 0, tileArrayPos.Value.y*mapRenderer.TileSize + mapRenderer.TileSize/2);
           //northwest, north. (up is -x) northeast, east. southeast, south. southwest, west.

           //ok yes this is hideous but the plan is to move to DOTS anyway so this is just a quick proof of concept 
           if (value.x > 0.8)
           {
               if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.NORTH)
               {
                   pos.z += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.EAST)
               {
                   pos.x += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTH)
               {
                   pos.z -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.WEST)
               {
                   pos.x -= mapRenderer.TileSize;
               }
           }
           if (value.x < -0.8)
           {
               if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.NORTH)
               {
                   pos.z -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.EAST)
               {
                   pos.x -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTH)
               {
                   pos.z += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.WEST)
               {
                   pos.x += mapRenderer.TileSize;
               }
           }
           if (value.y > 0.8)
           {
               if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.NORTH)
               {
                   pos.x -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.EAST)
               {
                   pos.z += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTH)
               {
                   pos.x += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.WEST)
               {
                   pos.z -= mapRenderer.TileSize;
               }
           }
           if (value.y < -0.8)
           {
               if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.NORTH)
               {
                   pos.x += mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.NORTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.EAST)
               {
                   pos.z -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHEAST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTH)
               {
                   pos.x -= mapRenderer.TileSize;
               }
               else if (camera.horizAngle == BattleGridCameraHorizontalAngle.SOUTHWEST || 
                   camera.horizAngle == BattleGridCameraHorizontalAngle.WEST)
               {
                   pos.z += mapRenderer.TileSize;
               }
           }
           //pass the coords of that tile to the focus. Or maybe don't. instead just pass the actual place it needs to go. Simpler.
           //cameraFocus.targetPos = pos;
       }
   }

   public void UpdateCameraZoom(InputAction.CallbackContext context)
   {
       //TODO: Figure out how we want controller zoom to work because this is really gross tbh
       //NOTE THAT THIS AFFECTS MOUSE, GET RID FO THIS AND FIGURE OUT HOW TO GET MAX/MIN TO WORK PROPERLY
       float value = context.ReadValue<float>() * 10f; //this is here because it's -1 to 1 despite the min/max being set to -120 to 120 (for gamepad triggers)
       //Debug.Log(value);
       if (value > 1) //mouse scroll inputs set it to +/-120 (this number is arbitrary probably, in terms of scroll wheeling)
       {
           camera.PreviousZoomLevel();
       }
       else if (value < -1)
       {
           camera.NextZoomLevel();
       }
   }*/
    }
}