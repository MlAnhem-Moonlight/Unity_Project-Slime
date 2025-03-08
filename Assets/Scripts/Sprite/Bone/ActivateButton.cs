using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateButton : MonoBehaviour
{
    public GameObject iconPrefab;

    private GameObject instantiatedIcon;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (instantiatedIcon != null )
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // Get the GameObject with tag "Player"
            instantiatedIcon.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && instantiatedIcon == null && this.enabled)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // Get the GameObject with tag "Player"
            instantiatedIcon = Instantiate(iconPrefab, new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z), Quaternion.identity);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && instantiatedIcon != null && this.enabled)
        {
            Destroy(instantiatedIcon);
            instantiatedIcon = null;
        }
    }
}


