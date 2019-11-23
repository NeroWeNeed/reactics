using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class BattleGridCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 5f;

    [SerializeField]
    private BattleGridCameraHorizontalAngle horizontalAngle = BattleGridCameraHorizontalAngle.NORTH;

    [SerializeField]
    private BattleGridCameraVerticalAngle verticalAngle = BattleGridCameraVerticalAngle.LOW;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool moving = false;
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
        if (!moving)
        {
            //TODO Camera doesn't display properly when doing topdown view

            double horizontalAngle = (Math.PI / 180.0) * ((int)cameraHorizontalAngle * (360 / horizontalRotations));
            double verticalAngle = (Math.PI / 180.0) * ((int)cameraVerticalAngle * (90 / (verticalRotations - 1)));

            double magnitude = Math.Sqrt(Math.Pow((double)battleGridManager.RealWidth, 2.0) + Math.Pow((double)battleGridManager.RealHeight, 2.0)) / 2.0;
            magnitude *= 1.25;
            Vector3 center = battleGridManager.Center();

            Vector3 newPosition = new Vector3(
                (float)(center.x + Math.Cos(horizontalAngle) * Math.Cos(verticalAngle) * magnitude),
            (float)(center.y + Math.Sin(verticalAngle) * magnitude),
            (float)(center.z - Math.Sin(horizontalAngle) * Math.Cos(verticalAngle) * magnitude)
            );
            //Debug.Log($"ANGLE: {Math.Atan2(newPosition.z - center.z, newPosition.x - center.x) * (180 / Math.PI) }");
            if (immediate)
            {
                targetPosition = newPosition;
                transform.position = targetPosition;
                transform.LookAt(new Vector3(
battleGridManager.gameObject.transform.position.x + (battleGridManager.RealWidth / 2),
battleGridManager.gameObject.transform.position.y,
battleGridManager.gameObject.transform.position.z + (battleGridManager.RealHeight / 2)
));
            }
            else
            {

                moving = newPosition != targetPosition;
                targetPosition = newPosition;
            }
            this.horizontalAngle = cameraHorizontalAngle;
            this.verticalAngle = cameraVerticalAngle;
        }
    }

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        battleGridManager = GetComponentInParent<BattleGridManager>();
        AlignCamera(horizontalAngle, verticalAngle, true);
        CenterCamera();
    }
    private void OnValidate()
    {
        if (battleGridManager == null)
            battleGridManager = GetComponentInParent<BattleGridManager>();
        AlignCamera(horizontalAngle, verticalAngle, true);
        CenterCamera();
    }


    public void UpdateCameraOrientation(InputAction.CallbackContext context)
    {

        Vector2 value = context.ReadValue<Vector2>();
        if (value.x > 0.5)
        {
            PrevHorizontalOrientation();
        }
        else if (value.x < -0.5)
        {
            NextHorizontalOrientation();
        }
        if (value.y > 0.5)
        {
            NextVerticalOrientation();
        }
        else if (value.y < -0.5)
        {
            PrevVerticalOrientation();
        }


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
        AlignCamera(horizontalAngle, (BattleGridCameraVerticalAngle)angle, false);
    }

    public void PrevVerticalOrientation()
    {
        int angle = ((int)verticalAngle) - 1;
        if (angle >= verticalRotations)
            angle = verticalRotations - 1;
        if (angle < 0)
            angle = 0;
        AlignCamera(horizontalAngle, (BattleGridCameraVerticalAngle)angle, false);
    }

    private void FixedUpdate()
    {

        /*                 if ((lastUpdate + 1) % 120 == 0)
                        {
                            NextHorizontalOrientation();
                        }
                        else
                            lastUpdate++; */
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
            transform.LookAt(new Vector3(
            battleGridManager.gameObject.transform.position.x + (battleGridManager.RealWidth / 2),
        battleGridManager.gameObject.transform.position.y,
        battleGridManager.gameObject.transform.position.z + (battleGridManager.RealHeight / 2)
        ));

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {

                moving = false;
            }
        }
    }

}
