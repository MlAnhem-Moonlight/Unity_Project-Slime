using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    void Update()
    {
        Vector3 clampedPosition = cameraTransform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minPosition.x, maxPosition.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minPosition.y, maxPosition.y);

        cameraTransform.position = clampedPosition;
    }
}

