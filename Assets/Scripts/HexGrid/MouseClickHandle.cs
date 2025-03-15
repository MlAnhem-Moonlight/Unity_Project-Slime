using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class MouseController : Singleton<MouseController>
{
    // Implement ISingleton interface methods here
    public Action<RaycastHit> onLeftMouseClick;
    public Action<RaycastHit> onRightMouseClick;
    public Action<RaycastHit> onMiddleMouseClick;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            CheckMouseClick(1);
        }
        if (Input.GetMouseButtonDown(2))
        {
            CheckMouseClick(2);
        }
    }

    void CheckMouseClick(int mouseButton)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (mouseButton == 0)
            {
                onLeftMouseClick?.Invoke(hit);
            }
            else if (mouseButton == 1)
            {
                onRightMouseClick?.Invoke(hit);
            }
            else if (mouseButton == 2)
            {
                onMiddleMouseClick?.Invoke(hit);
            }
        }
    }

}
