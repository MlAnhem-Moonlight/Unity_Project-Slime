using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public HexGrid hexGrid;
    private Character selectedCharacter;
    public float moveSpeed = 5f; // Speed of character movement
    private Coroutine currentMovementCoroutine; // Reference to the current movement coroutine

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click: Select character
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
        else if (Input.GetMouseButtonDown(1) && selectedCharacter != null) // Right click: Move character
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider != null)
            {
                HexTile targetTile = hit.collider.GetComponent<HexTile>();
                if (targetTile != null && !targetTile.IsOccupied)
                {
                    var path = Pathfinding.FindPath(hexGrid.Tiles, selectedCharacter.CurrentTile, targetTile);
                    if (path != null && path.Count > 0)
                    {
                        DrawPath(path); // Draw the calculated path
                        if (currentMovementCoroutine != null)
                        {
                            StopCoroutine(currentMovementCoroutine); // Stop any existing movement
                        }
                        currentMovementCoroutine = StartCoroutine(MoveAlongPath(selectedCharacter, path));
                    }
                    else Debug.Log("No path found!");
                }
            }
        }
    }

    IEnumerator MoveAlongPath(Character character, List<HexTile> path)
    {
        // Skip the first tile if it's the current tile
        int startIndex = path[0] == character.CurrentTile ? 1 : 0;

        // Mark the starting tile as not occupied
        if (character.CurrentTile != null)
        {
            character.CurrentTile.IsOccupied = false;
        }

        for (int i = startIndex; i < path.Count; i++)
        {
            HexTile nextTile = path[i];

            // Only move if the next tile is not occupied
            if (!nextTile.IsOccupied)
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
                if (character.CurrentTile != null)
                {
                    character.CurrentTile.IsOccupied = false;
                }
                character.CurrentTile = nextTile;
                character.CurrentTile.IsOccupied = true;

                // Reduce stamina for each tile moved
                character.ReduceStamina(1);
            }
            else
            {
                Debug.Log("Path blocked, stopping movement.");
                break;
            }
        }

        // Clear the current movement coroutine reference when done
        currentMovementCoroutine = null;
    }

    void DrawPath(List<HexTile> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i].WorldPosition, path[i + 1].WorldPosition, Color.green, 2f);
        }
    }
}
