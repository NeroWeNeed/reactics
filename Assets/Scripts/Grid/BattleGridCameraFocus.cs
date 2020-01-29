using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridCameraFocus : MonoBehaviour
{

    private Vector2 currentMousePosition;

    [SerializeField]
    private BattleGridCamera camera;

    private float cameraSpeed = 15f;

    private Vector2 leftStickVector = new Vector2(0, 0);

    public Vector3 targetPos = new Vector3();

    private float screenEdgeLength = 40f;
    
    private bool pointerPresent = false;
    void Start()
    {
        //TODO: Maybe add a reference to the battle grid or map or w/e and make it start in the center
    }


    public void SetCurrentMousePosition(Vector2 mousePosition)
    {
        currentMousePosition = mousePosition;
    }


    public void SetLeftStickStrength(Vector2 stickStrength)
    {
        leftStickVector = stickStrength;
    }

    public void TogglePointer(bool toggle)
    {
        pointerPresent = toggle;
    }

    ///This function just moves the camera focus (the thing the camera looks at) along the xz plane when the mouse hits the edges of the screen.
    ///It also causes this object to rotate around the Y axis with the camera so the movement calculations are easy
    ///Right now there's no limit to how far the camera can pan. Should clamp this at some point.
    void Update()
    {
        if (!camera.rotating)
        {
            //Rotate around same axis as camera
            transform.forward = new Vector3(camera.transform.forward.x, transform.forward.y, camera.transform.forward.z);

            if (pointerPresent)
            {
                //Pan based on mouse position
                if (currentMousePosition.y >= Screen.height - screenEdgeLength)
                    transform.Translate(0, 0, cameraSpeed, Space.Self);
                if (currentMousePosition.y <= screenEdgeLength)
                    transform.Translate(0, 0, -cameraSpeed, Space.Self);
                if (currentMousePosition.x >= Screen.width - screenEdgeLength)
                    transform.Translate(cameraSpeed, 0, 0, Space.Self);
                if (currentMousePosition.x <= screenEdgeLength)
                    transform.Translate(-cameraSpeed, 0, 0, Space.Self);
            }
            else
            {
                //technically could add a bool to this so that when it stops being true, the bool gets set and applies the "center to a tile" code.
                //seems like a hack though, maybe unnecessary when I integrate this nonsense to DOTS and it becomes clean and friendly and cool
                if (leftStickVector.magnitude > 0.1f)
                    transform.Translate(leftStickVector.x * cameraSpeed, 0, leftStickVector.y * cameraSpeed, Space.Self);
                else
                {
                    //first apply the Dpad input to move it a set distance
                    if (targetPos != null && targetPos.magnitude > 0)
                    {
                        transform.position = targetPos;
                        targetPos = new Vector3();
                    }
                    //then center it on that tile (which it should already be, if the last input was dpad.)
                }
            }
        }
    }
}
