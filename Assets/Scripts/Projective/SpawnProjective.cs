using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnProjective : MonoBehaviour
{
    public GameObject ptPrefab;
    public GameObject player;
    public KeyCode fireBtn;
    public float cooldownTime = 2f; // Public cooldown time in seconds

    private GameObject instantiatedIcon;
    private bool isOnCooldown = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (Input.GetKeyDown(fireBtn) && !isOnCooldown)
            {
                if (instantiatedIcon == null)
                {
                    instantiatedIcon = Instantiate(ptPrefab, new Vector3(player.transform.position.x + 1f, player.transform.position.y, player.transform.position.z), Quaternion.identity);
                    instantiatedIcon.transform.SetParent(transform);
                    FaceFollowMouse.instantiatedIcon = instantiatedIcon;
                    StartCoroutine(Cooldown());
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    // Coroutine to handle the cooldown
    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        Debug.Log("Cooldown finished");
    }
}

