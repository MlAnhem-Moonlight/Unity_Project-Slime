using UnityEngine;

public class HexTile : MonoBehaviour
{
    public int X; // Vị trí trong lưới (tọa độ X)
    public int Y; // Vị trí trong lưới (tọa độ Y)
    public Vector3 WorldPosition;
    public bool IsOccupied; // Ô này có nhân vật không?

    private HexGridInteraction hexGridInteraction;

    private void Start()
    {
        hexGridInteraction = FindObjectOfType<HexGridInteraction>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (hexGridInteraction != null)
            {
                //hexGridInteraction.HighlightHexTile(this);
            }
        }
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}
