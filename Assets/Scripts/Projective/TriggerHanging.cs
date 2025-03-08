using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHanging : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Hanging")
        {
            try
            {
                collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
                collision.gameObject.tag = "Untagged";
            }
            catch(Exception e)
            {
                Debug.Log("Hanging: " + e.Message);
            }
        }
    }
}
