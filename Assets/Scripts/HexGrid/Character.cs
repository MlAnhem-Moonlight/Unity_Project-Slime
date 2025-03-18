using UnityEngine;

public class Character : MonoBehaviour
{
    public HexTile CurrentTile; // Ô mà nhân vật đang đứng
    public string CharacterName; // Tên nhân vật (nếu cần)
    public int health = 100;
    public int stamina = 10; // Thể lực của nhân vật
    private bool isMyTurn = false;

    private void Start()
    {
        HexGrid hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            if (hexGrid.IsGridGenerated())
            {
                // Nếu lưới đã được tạo, gọi trực tiếp OnGridGenerated
                OnGridGenerated();
            }
            else
            {
                // Đăng ký sự kiện OnGridGenerated nếu lưới chưa tạo
                hexGrid.OnGridGenerated += OnGridGenerated;
                Debug.Log("HexGrid found, subscribing to OnGridGenerated.");
            }
        }
        else
        {
            Debug.LogError("HexGrid not found!");
        }
    }

    private void OnGridGenerated()
    {
        Debug.Log("OnGridGenerated event triggered.");
        // Tìm HexTile gần nhất và đặt làm CurrentTile
        CurrentTile = FindNearestHexTile();
        if (CurrentTile != null)
        {
            PlaceOnTile(CurrentTile);
            Debug.Log($"{CharacterName} placed on tile ({CurrentTile.X}, {CurrentTile.Y}).");
        }
        else
        {
            Debug.LogError("No HexTile found near the character!");
        }
    }

    public void PlaceOnTile(HexTile tile)
    {
        // Di chuyển nhân vật đến ô
        if (CurrentTile != null)
        {
            CurrentTile.IsOccupied = false; // Bỏ đánh dấu ô cũ
        }
        CurrentTile = tile;
        tile.IsOccupied = true; // Đánh dấu ô mới
        transform.position = tile.WorldPosition; // Đặt vị trí nhân vật tại tâm ô
    }

    public void MoveToTile(HexTile newTile)
    {
        if (!newTile.IsOccupied) // Chỉ di chuyển nếu ô không bị chiếm
        {
            if (CurrentTile != null)
            {
                CurrentTile.IsOccupied = false; // Bỏ đánh dấu ô cũ
            }
            CurrentTile = newTile;
            CurrentTile.IsOccupied = true; // Đánh dấu ô mới
            transform.position = newTile.WorldPosition; // Cập nhật vị trí
            Debug.Log($"{CharacterName} moved to tile ({newTile.X}, {newTile.Y}).");
        }
        else
        {
            Debug.Log($"Target tile ({newTile.X}, {newTile.Y}) is already occupied!");
        }
    }

    public void ReduceStamina(int amount)
    {
        stamina -= amount;
        if (stamina < 0) stamina = 0;
        Debug.Log($"{CharacterName} has {stamina} stamina left.");
    }

    private HexTile FindNearestHexTile()
    {
        HexTile[] tiles = FindObjectsOfType<HexTile>();
        Debug.Log($"Found {tiles.Length} HexTiles.");
        HexTile nearestTile = null;
        float minDistance = float.MaxValue;

        // Tìm ô Hex gần nhất
        foreach (HexTile tile in tiles)
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

    public void StartTurn()
    {
        isMyTurn = true;
        Debug.Log($"{CharacterName}'s turn started.");
        // Thêm các logic liên quan đến bắt đầu lượt
    }

    public void EndTurn()
    {
        isMyTurn = false;
        Debug.Log($"{CharacterName}'s turn ended.");
    }

    public void PerformAction(Character target)
    {
        if (isMyTurn)
        {
            // Ví dụ: Hành động tấn công
            int damage = 10;
            target.TakeDamage(damage);
            Debug.Log($"{CharacterName} attacked {target.CharacterName} for {damage} damage.");
        }
        else
        {
            Debug.Log("It's not your turn!");
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{CharacterName} took {damage} damage. Remaining health: {health}");
        if (health <= 0)
        {
            Debug.Log($"{CharacterName} has been defeated!");
            gameObject.SetActive(false); // Vô hiệu hóa nhân vật nếu bị đánh bại
        }
    }
}
