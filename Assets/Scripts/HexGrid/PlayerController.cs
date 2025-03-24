using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public HexGrid hexGrid;
    public Character selectedCharacter;
    public float moveSpeed = 5f; // Speed of character movement
    private Coroutine currentMovementCoroutine; // Reference to the current movement coroutine

    void Start()
    {
        if (hexGrid == null)
        {
            Debug.LogError("HexGrid is not assigned in the Inspector.");
        }
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1) && selectedCharacter != null) // Right click: Move character
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider != null)
            {
                HexTile targetTile = hit.collider.GetComponent<HexTile>();
                if (targetTile != null && !targetTile.IsOccupied)
                {
                    if (hexGrid != null && hexGrid.Tiles != null)
                    {
                        //Debug.Log("HexGrid and Tiles are properly assigned.");
                        var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, selectedCharacter.currentTile, targetTile);
                        if (path != null && path.Count > 0)
                        {
                            DrawPath(path); // Draw the calculated path
                            //LogPath(path); // Log the path to the console
                            if (currentMovementCoroutine != null)
                            {
                                StopCoroutine(currentMovementCoroutine); // Stop any existing movement
                            }
                            currentMovementCoroutine = StartCoroutine(MoveAlongPath(selectedCharacter, path));
                        }
                        else Debug.Log("No path found!");
                    }
                    else
                    {
                        Debug.LogError("HexGrid or Tiles are not assigned properly.");
                    }
                }
            }
        }
    }

    IEnumerator MoveAlongPath(Character character, List<HexTile> path)
    {
        // Skip the first tile if it's the current tile
        int startIndex = path[0] == character.currentTile ? 1 : 0;

        // Mark the starting tile as not occupied
        if (character.currentTile != null)
        {
            character.currentTile.IsOccupied = false;
        }

        for (int i = startIndex; i < path.Count; i++)
        {
            HexTile nextTile = path[i];

            // Only move if the next tile is not occupied and is adjacent
            if (!nextTile.IsOccupied && AreTilesAdjacent(character.currentTile, nextTile))
            {
                Vector3 startPosition = character.transform.position;
                Vector3 endPosition = nextTile.WorldPosition;
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                // Smoothly move to the next tile
                while (Vector3.Distance(character.transform.position, endPosition) > 0.01f)
                {
                    float distCovered = (Time.time - startTime) * moveSpeed;
                    float fractionOfJourney = distCovered / journeyLength;
                    character.transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }

                // Update the character's current tile
                if (character.currentTile != null)
                {
                    character.currentTile.IsOccupied = false;
                }
                character.currentTile = nextTile;
                character.currentTile.IsOccupied = true;

                // Reduce stamina for each tile moved
                character.ReduceStamina(1);
            }
            else
            {
                Debug.Log("Path blocked or tiles are not adjacent, stopping movement.");
                break;
            }
        }

        // Clear the current movement coroutine reference when done
        currentMovementCoroutine = null;
    }

    bool AreTilesAdjacent(HexTile currentTile, HexTile nextTile)
    {
        // Calculate the difference in coordinates
        int dx = Mathf.Abs(currentTile.X - nextTile.X);
        int dy = Mathf.Abs(currentTile.Y - nextTile.Y);

        // Check if the tiles are adjacent
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1) || (dx == 1 && dy == 1);
    }
    public IEnumerator MoveCharacterAlongPath(Character character, List<HexTile> path)
    {
        // Skip the first tile if it's the current tile
        int startIndex = path[0] == character.currentTile ? 1 : 0;

        // Mark the starting tile as not occupied
        if (character.currentTile != null)
        {
            character.currentTile.IsOccupied = false;
        }

        for (int i = startIndex; i < path.Count; i++)
        {
            HexTile nextTile = path[i];

            // Only move if the next tile is not occupied and is adjacent
            if (!nextTile.IsOccupied && AreTilesAdjacent(character.currentTile, nextTile))
            {
                Vector3 startPosition = character.transform.position;
                Vector3 endPosition = nextTile.WorldPosition;
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                // Smoothly move to the next tile
                while (Vector3.Distance(character.transform.position, endPosition) > 0.01f)
                {
                    float distCovered = (Time.time - startTime) * moveSpeed;
                    float fractionOfJourney = distCovered / journeyLength;
                    character.transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }

                // Update the character's current tile
                if (character.currentTile != null)
                {
                    character.currentTile.IsOccupied = false;
                }
                character.currentTile = nextTile;
                character.currentTile.IsOccupied = true;

                // Reduce stamina for each tile moved
                character.ReduceStamina(1);

                // Check if out of stamina
                if (character.currentStamina <= 0)
                {
                    Debug.Log("Out of stamina, ending movement.");
                    break;
                }
            }
            else
            {
                Debug.Log("Path blocked or tiles are not adjacent, stopping movement.");
                break;
            }
        }

        // Clear the current movement coroutine reference when done
        currentMovementCoroutine = null;
    }

    bool PathIsWithinStaminaRange(Character character, List<HexTile> path)
    {
        // Calculate the path length (minus the starting tile)
        int pathLength = path.Count > 0 && path[0] == character.currentTile
            ? path.Count - 1
            : path.Count;

        // Check if the character has enough stamina to complete the path
        return pathLength <= character.currentStamina;
    }

    void DrawPath(List<HexTile> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i].WorldPosition, path[i + 1].WorldPosition, Color.green, 2f);
        }
    }

    void LogPath(List<HexTile> path)
    {
        Debug.Log("Path found:");
        foreach (var tile in path)
        {
            Debug.Log($"Tile at ({tile.X}, {tile.Y})");
        }
    }
}
