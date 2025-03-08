using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Get the main camera if not set
        }
    }

    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            GameObject target = GetTargetUnderMouse();
            if (target != null)
            {
                Debug.Log("Target selected: " + target.name);
                // You can now use the target GameObject as needed
            }
        }*/
    }

    public GameObject GetTargetUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && !hit.collider.CompareTag("Wall"))
        {
            return hit.collider.gameObject;
        }

        return null;
    }
}
