using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class BattlePlayer : MonoBehaviour
{

    [SerializeField]
    private BattleGridCamera camera;
    private void Start()
    {

    }
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
}