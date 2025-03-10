using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjective : MonoBehaviour
{
    public float force = 10f; // Public force variable to control the force applied
    public MonoBehaviour script;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (script != null)
        {
            if (!script.enabled) ApplyForce();
        }
    }

    // Method to apply force in the direction the object is facing
    void ApplyForce()
    {
        Vector2 direction = transform.right; // Get the direction the object is facing
        rb.AddForce(direction * force, ForceMode2D.Impulse); // Apply the force
    }

    // Method to stop the object when it enters a trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Ally") && !other.CompareTag("AIX"))
        {
            if(other.CompareTag("Enemy") || other.CompareTag("Hanging")) Destroy(gameObject);
            StopAndDestroy();
        }
    }

    // Method to stop the object and destroy it after 3 seconds
    void StopAndDestroy()
    {
        transform.parent = null;
        rb.velocity = Vector2.zero; // Stop the object's movement
        rb.angularVelocity = 0f; // Stop any rotation
        StartCoroutine(IncreaseDragOverTime(1f)); // Start coroutine to increase drag over 0.2 seconds
        StartCoroutine(DestroyAfterDelay(1f)); // Start coroutine to destroy the object after 3 seconds
    }

    // Coroutine to increase drag over a specified duration
    IEnumerator IncreaseDragOverTime(float duration)
    {
        float startDrag = 100f;
        float endDrag = 1000f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rb.drag = Mathf.Lerp(startDrag, endDrag, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.drag = endDrag; // Ensure the drag is set to the final value
    }

    // Coroutine to destroy the object after a delay
    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
