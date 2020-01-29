using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Camera))]
public class BattleGridCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f; //Speed of camera pan

    [SerializeField]
    private float maxCameraRotationTime = 0.3f; //Time in seconds the camera takes to rotate from one position to the next
    [SerializeField]
    private float currentCameraRotationTime = 0f; //Current time in seconds that the camera has been rotating
    [SerializeField]
    private Vector3 lastCameraPosition;
    [SerializeField]
    private BattleGridCameraFocus cameraFocus;

    private float maxCameraZoom = 1f;

    private float minCameraZoom = 0.1f;

    private Vector3 offset;

    private float currentCameraZoom = 0.5f;

    [SerializeField]
    private BattleGridCameraHorizontalAngle horizontalAngle = BattleGridCameraHorizontalAngle.NORTH;

    public BattleGridCameraHorizontalAngle horizAngle => horizontalAngle;
    

    [SerializeField]
    private BattleGridCameraVerticalAngle verticalAngle = BattleGridCameraVerticalAngle.LOW;

    private Vector3 targetPosition;
    public bool rotating = false; //I made this public to fix an issue temporarily but that feels grimy and like there's a better way to go about this
    private BattleGridManager battleGridManager;

    private int horizontalRotations = Enum.GetValues(typeof(BattleGridCameraHorizontalAngle)).Length;
    private int verticalRotations = Enum.GetValues(typeof(BattleGridCameraVerticalAngle)).Length;


    public void CenterCamera()
    {
        transform.LookAt(new Vector3(
            battleGridManager.gameObject.transform.position.x + (battleGridManager.RealWidth / 2),
        battleGridManager.gameObject.transform.position.y,
        battleGridManager.gameObject.transform.position.z + (battleGridManager.RealHeight / 2)
        ));
    }

    public void AlignCamera(BattleGridCameraHorizontalAngle cameraHorizontalAngle, BattleGridCameraVerticalAngle cameraVerticalAngle = BattleGridCameraVerticalAngle.LOW, bool immediate = true)
    {
        if (!rotating)
        {
            transform.position = cameraFocus.transform.position + offset;
            double horizontalAngle = (Math.PI / 180.0) * ((int)cameraHorizontalAngle * (360 / horizontalRotations));
            if (cameraHorizontalAngle == BattleGridCameraHorizontalAngle.NORTHEAST ||
                (cameraHorizontalAngle == BattleGridCameraHorizontalAngle.NORTHWEST) ||
                (cameraHorizontalAngle == BattleGridCameraHorizontalAngle.SOUTHEAST) ||
                (cameraHorizontalAngle == BattleGridCameraHorizontalAngle.SOUTHWEST))
            {
                horizontalAngle += 0.22; //FE3H rule, could do a % here instead for less code but this seems more like... proper. idk
            }
            double verticalAngle = (Math.PI / 180.0) * (Mathf.Clamp((int)cameraVerticalAngle * (90 / (verticalRotations - 1)), 0, 89.9f));

            //Sets the camera's maximum zoom value (1 * magnitude) to a reasonable distance based on the size of the map.
            double magnitude = Math.Sqrt(Math.Pow(battleGridManager.RealWidth, 2.0) + Math.Pow(battleGridManager.RealHeight, 2.0)) / 2.0;
            //Apply camera zoom
            magnitude *= 1.25 * currentCameraZoom;
            
            //Get target position for camera to start moving to
            Vector3 newPosition = new Vector3(
                (float)(cameraFocus.transform.position.x + Math.Cos(horizontalAngle) * Math.Cos(verticalAngle) * magnitude),
                (float)(cameraFocus.transform.position.y + Math.Sin(verticalAngle) * magnitude),
                (float)(cameraFocus.transform.position.z - Math.Sin(horizontalAngle) * Math.Cos(verticalAngle) * magnitude)
            );

            if (immediate) //Move the camera to the position immediately (used for zooming, etc.)
            {
                targetPosition = newPosition;
                transform.position = targetPosition;
                offset = transform.position - cameraFocus.transform.position;
            }
            else //Move the camera to the position over time (used for camera rotation, etc.)
            {
                rotating = newPosition != targetPosition;
                targetPosition = newPosition;
                lastCameraPosition = transform.position;
            }

            //Set the new horizontal and vertical angles, if they changed.
            this.horizontalAngle = cameraHorizontalAngle;
            this.verticalAngle = cameraVerticalAngle;
        }
    }

    void Start()
    {
        //Calculate the offset from the point in space the camera is looking at
        offset = transform.position - cameraFocus.transform.position;

        targetPosition = transform.position;
        battleGridManager = GetComponentInParent<BattleGridManager>();
        AlignCamera(horizontalAngle, verticalAngle, true);
    }
    private void OnValidate()
    {
        if (battleGridManager == null)
            battleGridManager = GetComponentInParent<BattleGridManager>();
        AlignCamera(horizontalAngle, verticalAngle, true);
    }

    public void NextHorizontalOrientation()
    {
        int angle = ((((int)horizontalAngle) + 1) % horizontalRotations);
        AlignCamera((BattleGridCameraHorizontalAngle)angle, verticalAngle, false);
    }

    public void PrevHorizontalOrientation()
    {
        int angle = ((int)horizontalAngle) - 1;
        angle = ((angle % horizontalRotations) + horizontalRotations) % horizontalRotations;
        AlignCamera((BattleGridCameraHorizontalAngle)angle, verticalAngle, false);
    }

    public void NextVerticalOrientation()
    {
        int angle = ((int)verticalAngle) + 1;
        if (angle >= verticalRotations)
            angle = verticalRotations - 1;
        if (angle < 0)
            angle = 0;
        if (angle != (int)verticalAngle)
            AlignCamera(horizontalAngle, (BattleGridCameraVerticalAngle)angle, false);
    }

    public void PrevVerticalOrientation()
    {
        int angle = ((int)verticalAngle) - 1;
        if (angle >= verticalRotations)
            angle = verticalRotations - 1;
        if (angle < 0)
            angle = 0;
        if (angle != (int)verticalAngle)
            AlignCamera(horizontalAngle, (BattleGridCameraVerticalAngle)angle, false);
    }

    public void NextZoomLevel()
    {
        if (!rotating && currentCameraZoom < maxCameraZoom)
        {
            currentCameraZoom += 0.05f;
            AlignCamera(horizontalAngle, verticalAngle, true);
        }
    }

    public void PreviousZoomLevel()
    {
        if (!rotating && currentCameraZoom > minCameraZoom)
        {
            currentCameraZoom -= 0.05f;
            AlignCamera(horizontalAngle, verticalAngle, true);
        }
    }

    private void Update()
    {
        if (rotating)
        {
            currentCameraRotationTime += Time.deltaTime;
            transform.position = Vector3.Lerp(lastCameraPosition, targetPosition, currentCameraRotationTime/maxCameraRotationTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                rotating = false;
                offset = targetPosition - cameraFocus.transform.position;
                currentCameraRotationTime = 0;
            }
        }
        else
        {
            //If not rotating, move with the camera focus in case it's moving.
            transform.position = cameraFocus.transform.position + offset;
        }
        
        transform.LookAt(cameraFocus.transform);
    }

}
