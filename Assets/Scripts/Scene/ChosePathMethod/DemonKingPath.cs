using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonKingPath : MonoBehaviour
{
    public KeyCode destroyKey = KeyCode.E; // Key to destroy the instantiated icon
    public GameObject throne;

    private bool isPlayerNearby = false; // Biến để kiểm tra nếu nhân vật đang ở gần object

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (throne!=null) Destroy(throne);
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
        if(!other.CompareTag("Player") && !other.CompareTag("Projective")) GetComponent<Rigidbody2D>().gravityScale = 1;
    }
}
