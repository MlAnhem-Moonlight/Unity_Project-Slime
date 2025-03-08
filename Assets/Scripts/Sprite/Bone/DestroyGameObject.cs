using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    public KeyCode destroyKey = KeyCode.E; // Key to destroy the instantiated icon
    private bool isPlayerNearby = false; // Biến để kiểm tra nếu nhân vật đang ở gần object
    public GameObject ob;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if(ob != null)
            {
                ob.GetComponent<ActivateButton>().enabled = false;
                ob.GetComponent<DemonKingPath>().enabled = false;
            }
            Destroy(gameObject); // Hủy object
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
