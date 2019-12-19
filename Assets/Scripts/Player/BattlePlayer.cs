using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class BattlePlayer : MonoBehaviour
{

    [SerializeField]
    private BattleGridCamera camera;

    [SerializeField]
    private BattleGridCameraFocus cameraFocus;

    private void Start()
    {

    }

    //TODO: Figure out why all these methods get called twice for one input
    public void UpdateCameraOrientation(InputAction.CallbackContext context)
    {
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
    public void TileHover(InputAction.CallbackContext context)
    {
        cameraFocus.SetCurrentMousePosition(context.ReadValue<Vector2>());
        Debug.Log(context.ReadValue<Vector2>());
        /*do cursor stuff as well? these being two functions is maybe a better idea.
        like this function just holds the other two, one that deals with camera pans and one that deals with cursor positioning.
        just cohesion stuff I guess.*/
    }

    public void UpdateCameraZoom(InputAction.CallbackContext context)
    {
        //TODO: Figure out how to stop this from being called, or reset it when the camera gets to its spot, or just have it zoom AS the camera is rotating. last one seems best.
        float value = context.ReadValue<float>();
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