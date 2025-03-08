using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDownToShutScripts : MonoBehaviour
{
    public float timeToShut = 1f;
    public MonoBehaviour script;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(script.enabled)
        {
            if (timeToShut > 0)
            {
                timeToShut -= Time.deltaTime;

            }
            else script.enabled = false;
        }
    }
}
