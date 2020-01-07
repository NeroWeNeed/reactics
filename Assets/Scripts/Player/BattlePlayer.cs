using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(PlayerInput))]
public class BattlePlayer : MonoBehaviour
{

    [SerializeField]
    private BattleGridCamera camera;

    [SerializeField]
    private BattleGridCameraFocus cameraFocus;
    
    [SerializeField]
    private Cursor cursor;

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
            cameraFocus.TogglePointer(true);
            cursor.TogglePointer(true);
        }
        else
        {
            cameraFocus.TogglePointer(false);
            cursor.TogglePointer(false);
        }

        //Stops camera from moving if control scheme set to gamepad (probably a better way to do this?)
        InputUser.onChange += (user, change, device) => {
            if (change == InputUserChange.ControlSchemeChanged)
            {
                if (input.currentControlScheme == "Keyboard + Mouse")
                {
                    cameraFocus.TogglePointer(true);
                    cursor.TogglePointer(true);
                }
                else
                {
                    cameraFocus.TogglePointer(false);
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
            camera.PrevHorizontalOrientation();
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
            cameraFocus.SetCurrentMousePosition(value);
            cursor.SetCurrentMousePosition(value);
            Vector2Int value2 = new Vector2Int((int)value.x, (int)value.y);
            Debug.Log(battleGridManager.Grid.GetTile(value2));
        }
        else if (input.currentControlScheme == "Gamepad")
        {
            cameraFocus.SetLeftStickStrength(value);
        }
        
    }

    public void TileMovement(InputAction.CallbackContext context)
    {
        /*Vector2 value = context.ReadValue<Vector2>();
        Vector2Int value2 = new Vector2Int((int)value.x, (int)value.y);
        battleGridManager.Grid.GetTile(value2);*/
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
    }
}