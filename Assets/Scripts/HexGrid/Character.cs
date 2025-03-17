using UnityEngine;

public class Character : MonoBehaviour
{
    public HexTile CurrentTile; // Ô mà nhân vật đang đứng
    public string CharacterName; // Tên nhân vật (nếu cần)

    private void Start()
    {
        // Subscribe to the OnGridGenerated event
        HexGrid hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            hexGrid.OnGridGenerated += OnGridGenerated;
        }
    }

    private void OnGridGenerated()
    {
        // Find the nearest HexTile and set it as the CurrentTile
        CurrentTile = FindNearestHexTile();
        if (CurrentTile != null)
        {
            PlaceOnTile(CurrentTile);
        }
        else
        {
            Debug.LogError("No HexTile found near the character!");
        }
    }

    public void PlaceOnTile(HexTile tile)
    {
        // Di chuyển nhân vật đến ô
        CurrentTile = tile;
        tile.IsOccupied = true; // Đánh dấu ô đã bị chiếm
        transform.position = tile.WorldPosition; // Di chuyển nhân vật đến trung tâm ô
    }

    public void MoveToTile(HexTile newTile)
    {
        if (!newTile.IsOccupied) // Chỉ di chuyển nếu ô không bị chiếm
        {
            if (CurrentTile != null)
            {
                CurrentTile.IsOccupied = false; // Bỏ chiếm ô cũ
            }

            CurrentTile = newTile;
            CurrentTile.IsOccupied = true; // Đánh dấu ô mới
            transform.position = newTile.WorldPosition; // Cập nhật vị trí
        }
        else
        {
            Debug.Log("Target tile is already occupied!");
        }
    }


    private HexTile FindNearestHexTile()
    {
        HexTile nearestTile = null;
        float minDistance = float.MaxValue;

        // Find the nearest HexTile
        foreach (HexTile tile in FindObjectsOfType<HexTile>())
        {
            float distance = Vector3.Distance(transform.position, tile.WorldPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }
}
    