using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    private Vector2 currentMousePosition;

    private float rayDistance;

    [SerializeField]
    BattleGridCamera camera;

    [SerializeField]
    BattleGridCameraFocus cameraFocus;

    private BattleGridManager battleGridManager;

    private bool pointerPresent = false;

    void Start()
    {
        battleGridManager = GetComponentInParent<BattleGridManager>();
        //there's probably a better way to do this but right now this works soooooooooo yeah, math it up later
        rayDistance = (float)(Math.Sqrt(Math.Pow(battleGridManager.RealWidth, 2.0) + Math.Pow(battleGridManager.RealHeight, 2.0)) * 2);
    }

    public void SetCurrentMousePosition(Vector2 mousePosition)
    {
        currentMousePosition = mousePosition;
    }

    public void TogglePointer(bool toggle)
    {
        pointerPresent = toggle;
    }

    // Update is called once per frame
    void Update()
    {
        //while this does work keep in mind I still need to not use a mesh collider here because reasons
        //reasons being the battle grid itself should probably be a seperate thing other than just being a component on the battlegridmanager
        //that and a mesh collider maybe isn't the best way to go about this? though maybe it is idk, we'll see
        if (pointerPresent)
        {
            Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(currentMousePosition);
            if (battleGridManager.GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hitInfo, rayDistance))
            {
                transform.position = ray.GetPoint(hitInfo.distance);
            }
        }
        else
        {
            transform.position = cameraFocus.transform.position;
        }
    }
}
