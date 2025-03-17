using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] public int hexWidth;
    [SerializeField] public int hexHeight;
    [SerializeField] public int hexSize;
    [SerializeField] public GameObject hexPrefab;
    [SerializeField] public HexOrientation orientation;

    public List<HexTile> HexTiles = new List<HexTile>();
    public HexTile[,] Tiles;

    public delegate void GridGeneratedHandler();
    public event GridGeneratedHandler OnGridGenerated;

    // Start is called before the first frame update
    void Start()
    {
        GenerateHexGrid2D();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;
                for (int s = 0; s < HexMatrix.Corners(orientation, hexSize).Length; s++)
                {
                    Gizmos.DrawLine(
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[s % 6],
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[(s + 1) % 6]
                    );
                }
            }
        }
    }

    void GenerateHexGrid2D()
    {
        Tiles = new HexTile[hexWidth, hexHeight]; // Mảng 2D các HexTile

        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;
                GameObject hexTileObject = Instantiate(hexPrefab, centrePosition, Quaternion.identity, transform);
                HexTile tile = hexTileObject.GetComponent<HexTile>();

                tile.X = x;
                tile.Y = y;
                tile.WorldPosition = centrePosition;
                tile.IsOccupied = false;

                Tiles[x, y] = tile; // Gán vào mảng
                HexTiles.Add(tile); // Thêm vào danh sách
            }
        }

        // Notify listeners that the grid generation is complete
        OnGridGenerated?.Invoke();
    }

    /*
    void GenerateHexGrid()
    {
        // Clear existing hex tiles
        foreach (HexTile tile in HexTiles)
        {
            Destroy(tile.gameObject);
        }
        HexTiles.Clear();

        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                // Calculate the center position of the hex tile
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;

                // Instantiate a new hex tile GameObject
                GameObject hexTileObject = Instantiate(hexPrefab, centrePosition, Quaternion.identity, transform);

                // Get the HexTile component
                HexTile tile = hexTileObject.GetComponent<HexTile>();
                if (tile != null)
                {
                    tile.X = x;
                    tile.Y = y;
                    tile.WorldPosition = centrePosition;
                    tile.IsOccupied = false;

                    // Add the tile to the list
                    HexTiles.Add(tile);
                }
                else
                {
                    Debug.LogError("HexPrefab does not have a HexTile component.");
                }
            }
        }
    }
    */
}

public enum HexOrientation
{
    PointyTop,
    FlatTop
}
