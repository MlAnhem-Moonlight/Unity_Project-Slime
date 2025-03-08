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
    private int direction = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(player.GetComponent<Movement>().IsFacingLeft) direction = -1;
        else direction = 1;
        if (Input.GetKeyDown(fireBtn) && !isOnCooldown)
        {
            if (instantiatedIcon == null)
            {
                instantiatedIcon = Instantiate(ptPrefab, new Vector3(player.transform.position.x +1f, player.transform.position.y + 1f, player.transform.position.z), Quaternion.identity);
                FaceFollowMouse.instantiatedIcon = instantiatedIcon;
                StartCoroutine(Cooldown());
            }
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

