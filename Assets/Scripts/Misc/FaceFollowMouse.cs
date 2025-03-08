using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceFollowMouse : MonoBehaviour
{
    public static GameObject instantiatedIcon;

    private GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the z-coordinate is zero

        // Calculate the direction to the mouse
        Vector2 directionToMouse = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        // Rotate the object to face the mouse
        transform.rotation = Quaternion.Euler(Vector3.forward * angle);

        instantiatedIcon.transform.position = new Vector3(player.transform.position.x + 1f, player.transform.position.y + 1f, player.transform.position.z);
   
    }
}
