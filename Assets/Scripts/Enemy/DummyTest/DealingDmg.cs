using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealingDmg : MonoBehaviour
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
       
        if (collision.gameObject.layer != gameObject.layer)
        {
            try
            {
                collision.gameObject.GetComponent<TakenDmg>().TakeDamage(10);
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }
        }
    }



}
