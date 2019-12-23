using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridCameraFocus : MonoBehaviour
{

    private Vector2 currentMousePosition;

    [SerializeField]
    private BattleGridCamera camera;

    private float cameraSpeed = 10f;

    private float screenEdgeLength = 30f;
    void Start()
    {
        //Maybe add a reference to the battle grid and make it start in the center?
    }


    public void SetCurrentMousePosition(Vector2 mousePosition)
    {
        currentMousePosition = mousePosition;
    }

    ///This function just moves the camera focus (the thing the camera looks at) along the xz plane when the mouse hits the edges of the screen.
    ///It also causes this object to rotate around the Y axis with the camera so the movement calculations are easy
    ///Right now there's no limit to how far the camera can pan. Should clamp this at some point.
    void Update()
    {
        if (!camera.rotating) //temporary fix, I don't like setting this to public (I don't actually know why that's a terrible idea, but something tells me this is like wrong in principle and can be done better)
        {
            //Rotate around y axis same as camera
            transform.forward = new Vector3(camera.transform.forward.x, transform.forward.y, camera.transform.forward.z);

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
    }
}
