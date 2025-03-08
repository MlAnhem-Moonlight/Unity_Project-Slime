using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakenDmg : MonoBehaviour
{
    public GameObject floatingText;
    public int health = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(int damage)
    {
        if(floatingText != null)
        {
            Debug.Log("Floating text");
            Vector3 vector3 = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            GameObject floatingTxt = Instantiate(floatingText, vector3, Quaternion.identity);
            floatingTxt.GetComponentInChildren<TextMesh>().text = damage.ToString();
        }
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Xử lý khi kẻ địch chết
        Destroy(gameObject);
    }
}
