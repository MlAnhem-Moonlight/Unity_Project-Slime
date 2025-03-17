using UnityEngine;

public class HexagonHover : MonoBehaviour
{
    private Color originalColor;
    private Renderer hexRenderer;

    private void Start()
    {
        hexRenderer = GetComponent<Renderer>();
        if (hexRenderer != null)
        {
            originalColor = hexRenderer.material.color;
        }
    }

    private void OnMouseEnter()
    {
        if (hexRenderer != null)
        {
            hexRenderer.material.color = Color.green;
        }
    }

    private void OnMouseExit()
    {
        if (hexRenderer != null)
        {
            hexRenderer.material.color = originalColor;
        }
    }
}
