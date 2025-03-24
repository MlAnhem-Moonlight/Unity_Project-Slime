using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // For UI components

public class TurnManager : MonoBehaviour
{
    public PlayerController playerController;
    public List<Character> allCharacters = new List<Character>();
    public float turnDelay = 0.5f; // Delay between turns in seconds

    [Header("UI References")]
    public Text currentTurnText; // UI text to display current character's turn
    public GameObject gameOverPanel; // Panel to show when game ends
    public Text gameResultText; // Text to display win/loss message

    private List<Character> turnOrder = new List<Character>();
    private int currentTurnIndex = 0;
    private bool gameEnded = false;
    private bool isTurnActive = false;

    // Teams
    public string playerTeam = "Player";
    public string enemyTeam = "Enemy";

    void Start()
    {
        // Make sure references are set
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned to TurnManager!");
        }

        // Start the game
        InitializeGame();
    }

    public void InitializeGame()
    {
        // Reset game state
        gameEnded = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Find all characters in the scene if not manually assigned
        if (allCharacters.Count == 0)
        {
            allCharacters = FindObjectsOfType<Character>().ToList();
        }

        // Generate the first turn order
        GenerateRandomTurnOrder();

        // Start the first turn
        StartCoroutine(StartNextTurn());
    }

    void GenerateRandomTurnOrder()
    {
        // Clear the previous turn order
        turnOrder.Clear();

        // Create a temporary list of alive characters
        List<Character> aliveCharacters = allCharacters.Where(c => c.IsAlive()).ToList();

        // Randomize the turn order
        while (aliveCharacters.Count > 0)
        {
            int randomIndex = Random.Range(0, aliveCharacters.Count);
            turnOrder.Add(aliveCharacters[randomIndex]);
            aliveCharacters.RemoveAt(randomIndex);
        }

        // Reset the turn index
        currentTurnIndex = 0;

        // Debug the turn order
        string debugOrder = "Turn Order: ";
        foreach (var character in turnOrder)
        {
            debugOrder += character.name + " -> ";
        }
        Debug.Log(debugOrder);
    }

    IEnumerator StartNextTurn()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(turnDelay);

        // Check for game over condition
        if (CheckGameOver())
        {
            yield break;
        }

        // End of round, generate new turn order
        if (currentTurnIndex >= turnOrder.Count)
        {
            GenerateRandomTurnOrder();
        }

        // Get the character whose turn it is
        Character currentCharacter = turnOrder[currentTurnIndex];

        // Skip dead characters
        if (!currentCharacter.IsAlive())
        {
            currentTurnIndex++;
            StartCoroutine(StartNextTurn());
            yield break;
        }

        // Automatically select the character in the PlayerController
        playerController.selectedCharacter = currentCharacter;

        // Reset character's action points/stamina for this turn
        currentCharacter.ResetStaminaForTurn();

        // Update UI to show whose turn it is
        if (currentTurnText != null)
        {
            currentTurnText.text = "Current Turn: " + currentCharacter.name;
        }

        Debug.Log("Starting turn for " + currentCharacter.name);

        // Set turn as active
        isTurnActive = true;

        // If the character is an AI, handle its turn automatically
        if (currentCharacter.isAI)
        {
            yield return StartCoroutine(HandleAITurn(currentCharacter));
            EndTurn();
        }
        // For player-controlled characters, the PlayerController will handle the turn
        // The player must call EndTurn() when done
    }

    IEnumerator HandleAITurn(Character aiCharacter)
    {
        Debug.Log("AI turn: " + aiCharacter.name);

        // Wait a moment to make the AI's actions visible
        yield return new WaitForSeconds(1.0f);

        // Example AI behavior - find nearest enemy and move towards them
        Character nearestEnemy = FindNearestEnemy(aiCharacter);

        if (nearestEnemy != null)
        {
            // Example: Move towards enemy
            HexTile targetTile = FindBestTileTowardsEnemy(aiCharacter, nearestEnemy);

            if (targetTile != null)
            {
                var hexGrid = playerController.hexGrid;
                var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, targetTile);

                if (path != null && path.Count > 0)
                {
                    // Use the same movement coroutine logic from PlayerController
                    yield return StartCoroutine(playerController.MoveCharacterAlongPath(aiCharacter, path));
                }
            }

            // Add attack logic here if in range
            if (IsInAttackRange(aiCharacter, nearestEnemy))
            {
                Attack(aiCharacter, nearestEnemy);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    // Helper AI methods
    private Character FindNearestEnemy(Character aiCharacter)
    {
        Character nearest = null;
        float minDistance = float.MaxValue;

        foreach (var character in allCharacters)
        {
            if (character.IsAlive() && character.team != aiCharacter.team)
            {
                float distance = Vector3.Distance(aiCharacter.transform.position, character.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = character;
                }
            }
        }

        return nearest;
    }

    private HexTile FindBestTileTowardsEnemy(Character aiCharacter, Character enemy)
    {
        // This is a simple implementation, you might want to enhance this
        // with better pathfinding and tactical decision making

        HexGrid hexGrid = playerController.hexGrid;
        List<HexTile> adjacentTiles = hexGrid.GetAdjacentTiles(aiCharacter.currentTile);

        HexTile bestTile = null;
        float minDistance = float.MaxValue;

        foreach (var tile in adjacentTiles)
        {
            if (!tile.IsOccupied)
            {
                float distance = Vector3.Distance(tile.WorldPosition, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestTile = tile;
                }
            }
        }

        return bestTile;
    }

    private bool IsInAttackRange(Character attacker, Character target)
    {
        // Example implementation - check if target is within 1 tile
        return Vector3.Distance(attacker.transform.position, target.transform.position) <= 2.0f;
    }

    private void Attack(Character attacker, Character target)
    {
        Debug.Log(attacker.name + " attacks " + target.name);

        // Calculate damage - example implementation
        int damage = attacker.attackPower;

        // Apply damage to target
        target.TakeDamage(damage);

        // Visual feedback
        // You could add effects here
    }

    public void EndTurn()
    {
        if (!isTurnActive)
            return;

        isTurnActive = false;

        // Move to the next character
        currentTurnIndex++;

        // Start the next turn
        StartCoroutine(StartNextTurn());
    }

    bool CheckGameOver()
    {
        if (gameEnded)
            return true;

        // Check if all player characters are dead
        bool anyPlayerAlive = allCharacters.Any(c => c.team == playerTeam && c.IsAlive());

        // Check if all enemy characters are dead
        bool anyEnemyAlive = allCharacters.Any(c => c.team == enemyTeam && c.IsAlive());

        if (!anyPlayerAlive || !anyEnemyAlive)
        {
            gameEnded = true;

            // Show game over UI
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameResultText != null)
                {
                    if (!anyPlayerAlive)
                    {
                        gameResultText.text = "Game Over - You Lost!";
                    }
                    else
                    {
                        gameResultText.text = "Victory!";
                    }
                }
            }

            Debug.Log("Game Over! " + (anyPlayerAlive ? "Player wins!" : "Enemy wins!"));
            return true;
        }

        return false;
    }

    // Public method to force-end a turn (can be called from UI)
    public void ForceEndTurn()
    {
        if (isTurnActive)
        {
            EndTurn();
        }
    }
    // Property to check if a turn is active
    public bool IsTurnActive
    {
        get { return isTurnActive; }
    }

    // Check if it's a specific character's turn
    public bool IsCharacterTurn(Character character)
    {
        if (currentTurnIndex < 0 || currentTurnIndex >= turnOrder.Count)
            return false;

        return turnOrder[currentTurnIndex] == character;
    }

    // Get the character whose turn it currently is
    public Character GetCurrentTurnCharacter()
    {
        if (currentTurnIndex < 0 || currentTurnIndex >= turnOrder.Count)
            return null;

        return turnOrder[currentTurnIndex];
    }

    // Add a UI button to end the current turn
    public void EndTurnButton()
    {
        EndTurn();
    }
}