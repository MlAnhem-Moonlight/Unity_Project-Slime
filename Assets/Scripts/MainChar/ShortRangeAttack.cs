using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortRangeAttack : MonoBehaviour
{
    public float attackCooldown = 1f; // Thời gian chờ giữa các lần tấn công
    public int attackDamage = 10; // Sát thương của đòn tấn công
    public string enemyTag = "Enemy"; // Tag của kẻ địch
    public BoxCollider2D attackCollider; // Collider dùng để xác định phạm vi tấn công

    private float atkTime = 0;
    public bool Atk = false;

    public GameObject target = null;

    public KeyCode atkKey;

    public Animator isAtk;

    private TargetSelector targetSelector;

    void Start()
    {
        targetSelector = GetComponent<TargetSelector>();
    }

    void Update()
    {
        if (atkTime >= 0.5f && isAtk.GetBool("IsAtk") == true)
        {
            isAtk.SetBool("IsAtk", false);
        }
        else if (atkTime < attackCooldown)
        {
            Atk = false;
            atkTime += Time.deltaTime;
        }
        else
        {
            Atk = true;
        }

        if (Input.GetKeyDown(atkKey) && Atk == true)
        {
            atkTime = 0f;
            isAtk.SetBool("IsAtk", true);
        }

        setTarget();
    }

    // This method will be called by the animation event
    public void PerformAttack()
    {
        if (target == null)
        {
            // Lấy tất cả các đối tượng trong phạm vi tấn công
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackCollider.bounds.center, attackCollider.bounds.size, 0f);

            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag(enemyTag))
                {
                    // Gọi hàm để gây sát thương cho kẻ địch
                    try
                    {
                        collider.GetComponent<TakenDmg>().TakeDamage(attackDamage);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Atk all: "+e.Message);
                    }
                }
            }
        }
        else
        {
            try
            {
                target.GetComponent<TakenDmg>().TakeDamage(attackDamage);
            }
            catch (Exception e)
            {
                Debug.Log("Atk target: " + e.Message);
            }
        }
    }

    void setTarget()
    {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            target = targetSelector.GetTargetUnderMouse();
        }
    }
}
