using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Material material;
    public float distance;

    [Range(0f,0.5f)]
    public float spd = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        distance += spd * Time.deltaTime;
        material.SetTextureOffset("_MainTex", Vector2.right * distance);
    }
}
