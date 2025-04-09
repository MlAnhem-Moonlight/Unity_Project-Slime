using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakenDmg : MonoBehaviour
{
    public GameObject floatingText; // Prefab của floating text
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
        if (floatingText != null)
        {
            Debug.Log("Floating text");

            // Vị trí để floating text xuất hiện
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z);

            // Tạo floating text
            GameObject floatingTxt = Instantiate(floatingText, spawnPosition, Quaternion.identity);

            // Set floating text là con của object chứa script
            floatingTxt.transform.SetParent(this.transform);

            // Cập nhật nội dung của floating text
            floatingTxt.GetComponentInChildren<TextMesh>().text = damage.ToString();
        }

        health -= damage; // Trừ máu
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
