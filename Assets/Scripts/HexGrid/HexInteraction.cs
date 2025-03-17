using UnityEngine;

public class HexGridInteraction : MonoBehaviour
{
    public HexGrid hexGrid; // Tham chiếu đến lưới hex của bạn
    public LayerMask gridLayer; // Layer của lưới để xác định va chạm

    public GameObject highlightPrefab; // Prefab highlight (có SpriteRenderer)
    public GameObject currentHighlight; 

    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0)) // Nhấn chuột trái
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayer))
            {
                Vector3 hitPoint = hit.point;
                
                // Tính toán tọa độ của ô dựa trên vị trí va chạm
                HexTile selectedTile = GetHexTileFromWorldPosition(hitPoint);
                
                if (selectedTile != null)
                {
                    HighlightHexTile(selectedTile);
                    Debug.Log($"Clicked on HexTile at ({selectedTile.X}, {selectedTile.Y})");
                }
            }
        }
        */
    }



    HexTile GetHexTileFromWorldPosition(Vector3 worldPosition)
    {

        // Lặp qua danh sách các ô trong lưới
        foreach (HexTile tile in hexGrid.HexTiles)
        {
            float distance = Vector3.Distance(tile.WorldPosition, worldPosition);
            Debug.Log($"Distance to tile at ({tile.X}, {tile.Y}): {distance}");

            if (distance < hexGrid.hexSize * 0.9f) // Bán kính lục giác
            {
                return tile; // Trả về ô gần nhất
            }
        }
        return null; // Không tìm thấy ô
    }

    public void HighlightHexTile(HexTile tile)
    {
        // Xóa highlight cũ nếu có
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }

        // Tạo highlight tại ô được chọn
        currentHighlight = Instantiate(highlightPrefab, tile.WorldPosition, Quaternion.identity);
        currentHighlight.transform.localScale = Vector3.one * hexGrid.hexSize; // Căn chỉnh kích thước
    }
}
