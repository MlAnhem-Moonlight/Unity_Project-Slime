using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public HexGrid hexGrid;
    private Character selectedCharacter;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Chuột trái: Chọn nhân vật
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                
                Character character = hit.collider.GetComponent<Character>();
                
                if (character != null)
                {
                    selectedCharacter = character;
                    Debug.Log($"Selected character {selectedCharacter}");
                    Debug.Log($"Selected character at ({character.CurrentTile.X}, {character.CurrentTile.Y})");
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && selectedCharacter != null) // Chuột phải: Di chuyển nhân vật
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                HexTile targetTile = hit.collider.GetComponent<HexTile>();
                if (targetTile != null && !targetTile.IsOccupied)
                {
                    var path = Pathfinding.FindPath(hexGrid.Tiles, selectedCharacter.CurrentTile, targetTile);
                    if (path != null)
                    {
                        StartCoroutine(MoveAlongPath(selectedCharacter, path));
                    }
                }
            }
        }
    }

    System.Collections.IEnumerator MoveAlongPath(Character character, List<HexTile> path)
    {
        foreach (HexTile tile in path)
        {
            character.MoveToTile(tile);
            yield return new WaitForSeconds(0.2f); // Delay giữa các bước di chuyển
        }
    }
}
