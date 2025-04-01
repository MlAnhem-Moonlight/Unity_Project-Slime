using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName;
    public string team; // "Player" or "Enemy"
    public bool isAI = false; // Whether this character is controlled by AI

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int maxStamina = 5;
    public int currentStamina;
    public int attackPower = 20;
    public int defense = 10;

    [Header("Position")]
    public HexTile currentTile; // Tile where character is standing

    private bool isMyTurn = false;

    private void Start()
    {
        // Initialize stats
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        characterName = name; // Use object name as default character name

        // Check the tag of the object and set the team accordingly
        if (CompareTag("Ally") || CompareTag("Player"))
        {
            team = "Player";
        }
        else if (CompareTag("Enemy"))
        {
            team = "Enemy";
            isAI = true; // Enemies are controlled by AI
        }
        else
        {
            Debug.LogWarning("Character tag is neither 'Ally' nor 'Enemy'.");
        }

        HexGrid hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            if (hexGrid.IsGridGenerated())
            {
                // If grid is already generated, call OnGridGenerated directly
                OnGridGenerated();
            }
            else
            {
                // Register for OnGridGenerated event if grid isn't created yet
                hexGrid.OnGridGenerated += OnGridGenerated;
            }
        }
        else
        {
            Debug.LogError("HexGrid not found!");
        }
    }

    private void OnGridGenerated()
    {
        // Find nearest HexTile and set as currentTile
        currentTile = FindNearestHexTile();
        if (currentTile != null)
        {
            PlaceOnTile(currentTile);
            Debug.Log($"{characterName} placed on tile ({currentTile.X}, {currentTile.Y}).");
        }
        else
        {
            Debug.LogError("No HexTile found near the character!");
        }
    }

    public void PlaceOnTile(HexTile tile)
    {
        // Move character to tile
        if (currentTile != null)
        {
            currentTile.IsOccupied = false; // Unmark old tile
        }
        currentTile = tile;
        tile.IsOccupied = true; // Mark new tile
        transform.position = tile.WorldPosition; // Set character position at tile center
    }

    public void MoveToTile(HexTile newTile)
    {
        if (!newTile.IsOccupied) // Only move if tile is not occupied
        {
            if (currentTile != null)
            {
                currentTile.IsOccupied = false; // Unmark old tile
            }
            currentTile = newTile;
            currentTile.IsOccupied = true; // Mark new tile
            transform.position = newTile.WorldPosition; // Update position
            Debug.Log($"{characterName} moved to tile ({newTile.X}, {newTile.Y}).");
        }
        else
        {
            Debug.Log($"Target tile ({newTile.X}, {newTile.Y}) is already occupied!");
        }
    }

    // Reduce stamina when moving or performing actions
    public void ReduceStamina(int amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        Debug.Log($"{characterName} stamina reduced to {currentStamina}");
    }

    // Reset stamina at the start of the character's turn
    public void ResetStaminaForTurn()
    {
        currentStamina = maxStamina;
        Debug.Log($"{characterName} stamina reset to {currentStamina}");
    }

    private HexTile FindNearestHexTile()
    {
        HexTile[] tiles = FindObjectsOfType<HexTile>();
        Debug.Log($"Found {tiles.Length} HexTiles.");
        HexTile nearestTile = null;
        float minDistance = float.MaxValue;

        // Find nearest Hex tile
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
        ResetStaminaForTurn();
        Debug.Log($"{characterName}'s turn started.");
        // Add additional turn start logic here
    }

    public void EndTurn()
    {
        isMyTurn = false;
        Debug.Log($"{characterName}'s turn ended.");
    }

    // Attack another character
    public void Attack(Character target)
    {
        if (!IsAlive() || !target.IsAlive())
            return;

        if (isMyTurn)
        {
            Debug.Log($"{characterName} attacks {target.characterName}");
            target.TakeDamage(attackPower);
        }
        else
        {
            Debug.Log("It's not your turn!");
        }
    }

    // Take damage from attacks
    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            Debug.LogWarning("Damage cannot be negative.");
            return;
        }

        // Apply defense to reduce damage
        int actualDamage = Mathf.Max(1, damage - defense / 2);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);

        Debug.Log($"{characterName} took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");

        // Check if the character died
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Check if the character is still alive
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    // Handle death
    private void Die()
    {
        Debug.Log($"{characterName} has died!");

        // Clear the tile's occupied status
        if (currentTile != null)
        {
            currentTile.IsOccupied = false;
        }

        // Visual indication of death
        // You could trigger death animation, change sprite color, etc.
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        // Optionally disable collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        gameObject.SetActive(false); // Disable character if defeated
    }
}