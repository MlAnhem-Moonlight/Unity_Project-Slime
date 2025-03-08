using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTag : MonoBehaviour
{
    public string layerName;
    /*
    public string tagEnemy = "Enemy";
    public string tagAlly = "Ally";
    public string tagNeutral = "Neutral";
    */

    // Start is called before the first frame update
    void Start()
    {
        //ChangeLayerToTag(layerName, "Enemy");
        //ChangeLayerToTag(layerName, "Ally");
        //ChangeLayerToTag(layerName, "Neutral");
    }

    public static void ChangeLayerToTag(string layer, string tag)
    {
        int layerId = LayerMask.NameToLayer(layer);
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layerId)
            {
                obj.tag = tag;
            }
        }
    }
}
