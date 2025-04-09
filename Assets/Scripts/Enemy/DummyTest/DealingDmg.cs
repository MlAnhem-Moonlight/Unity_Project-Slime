using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealingDmg : MonoBehaviour
{
    public float knockbackForce = 5f; // Lực đẩy ngược (có thể tùy chỉnh)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != gameObject.layer && collision.gameObject.GetComponent<TakenDmg>() != null)
        {
            try
            {
                // Gây damage
                collision.gameObject.GetComponent<TakenDmg>().TakeDamage(10);

                // Tính lực đẩy ngược (knockback)
                ApplyKnockback(collision.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }
        }
    }

    void ApplyKnockback(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>(); // Lấy Rigidbody của đối tượng bị đẩy

        if (targetRb != null)
        {
            Rigidbody2D attackerRb = GetComponent<Rigidbody2D>(); // Lấy Rigidbody của đối tượng gây damage
            float attackerMass = attackerRb != null ? attackerRb.mass : 1f; // Nếu không có Rigidbody, đặt khối lượng mặc định = 1

            Vector2 knockbackDirection = (target.transform.position - transform.position).normalized; // Hướng đẩy ngược
            float massFactor = targetRb.mass / attackerMass; // Hệ số khối lượng: dựa trên mass của đối tượng bị đẩy và gây damage

            targetRb.AddForce(knockbackDirection * knockbackForce * massFactor, ForceMode2D.Impulse); // Áp dụng lực đẩy ngược
        }
    }
}
