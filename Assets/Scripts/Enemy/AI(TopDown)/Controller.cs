using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // References
    private HexGrid hexGrid;
    private TurnManager turnManager;

    // Q-Learning
    private Dictionary<(string, string), float> qTable = new Dictionary<(string, string), float>();
    private float learningRate = 0.1f;
    private float discountFactor = 0.9f;
    private float explorationRate = 0.2f;

    // DDA (Dynamic Difficulty Adjustment)
    private int aiDifficulty = 1; // Difficulty level (1 easy - 5 hard)
    private int playerWinCount = 0;
    private int playerLossCount = 0;

    // Character stats
    [Tooltip("Attack range of AI characters")]
    public int attackRange = 1;

    // Movement
    [Tooltip("Speed of AI character movement")]
    public float moveSpeed = 5f;
    private Coroutine currentMovementCoroutine;

    public List<ActionData> availableActions;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        hexGrid = FindObjectOfType<HexGrid>();
        // Initialize available actions
        availableActions = new List<ActionData>
        {
            new ActionData("Attack", damage: 20, stamina: 2, positionAdvantage: 1),
            new ActionData("Defense", damage: 0, stamina: 1, positionAdvantage: 2),
            new ActionData("Move", damage: 0, stamina: 1, positionAdvantage: 0)
        };
    }

    public string ChooseBestAction(string state)
    {
        // Exploration: Try a random action
        if (Random.value < explorationRate)
            return availableActions[Random.Range(0, availableActions.Count)].ActionName;

        // Use Utility AI to evaluate scores
        Dictionary<string, float> actionScores = new Dictionary<string, float>();
        foreach (var action in availableActions)
        {
            float qValue = qTable.ContainsKey((state, action.ActionName)) ? qTable[(state, action.ActionName)] : 0;
            actionScores[action.ActionName] = qValue + EvaluateAction(action);
        }

        // Choose action with highest score
        string bestAction = "";
        float bestScore = float.MinValue;
        foreach (var kvp in actionScores)
        {
            if (kvp.Value > bestScore)
            {
                bestScore = kvp.Value;
                bestAction = kvp.Key;
            }
        }

        return bestAction;
    }

    private float EvaluateAction(ActionData action)
    {
        // Adjust weights based on AI difficulty
        float weightAttack = 1.5f + (aiDifficulty * 0.5f);
        float weightDefense = 1f;
        float weightStamina = 0.5f;

        return (action.Damage * weightAttack) - (action.Stamina * weightStamina) + (action.PositionAdvantage * weightDefense);
    }

    public void UpdateQTable(string state, string action, float reward, string nextState)
    {
        float maxFutureQ = 0;
        foreach (var act in availableActions)
        {
            if (qTable.ContainsKey((nextState, act.ActionName)))
                maxFutureQ = Mathf.Max(maxFutureQ, qTable[(nextState, act.ActionName)]);
        }

        float currentQ = qTable.ContainsKey((state, action)) ? qTable[(state, action)] : 0;
        float newQ = currentQ + learningRate * (reward + discountFactor * maxFutureQ - currentQ);
        qTable[(state, action)] = newQ;
    }

    public void AdjustDifficulty()
    {
        if (playerWinCount >= 3)
        {
            aiDifficulty++;
            playerWinCount = 0;
        }
        if (playerLossCount >= 3)
        {
            aiDifficulty--;
            playerLossCount = 0;
        }
        aiDifficulty = Mathf.Clamp(aiDifficulty, 1, 5);

        Debug.Log($"AI Difficulty Adjusted: {aiDifficulty}");

        // Adjust exploration rate based on difficulty
        explorationRate = Mathf.Max(0.1f, 0.3f - (aiDifficulty * 0.05f));
    }

    public IEnumerator HandleAITurn(Character aiCharacter)
    {
        Debug.Log("AI turn: " + aiCharacter.characterName);

        // Cập nhật attackRange từ AtkRange của aiCharacter
        attackRange = aiCharacter.AtkRange;
        Debug.Log($"Updated attackRange to {attackRange} for {aiCharacter.characterName}");

        // Initial delay for player to see AI's turn start
        yield return new WaitForSeconds(0.5f);

        aiCharacter.StartTurn();

        // Find the nearest enemy
        Character nearestEnemy = FindNearestEnemy(aiCharacter);

        if (nearestEnemy != null)
        {
            // Get current state representation
            string state = GetStateRepresentation(aiCharacter, nearestEnemy);

            // Choose best action based on current state
            string chosenAction = ChooseBestAction(state);
            Debug.Log($"AI chose action: {chosenAction}");

            // Execute the chosen action
            if (chosenAction == "Attack" && IsInAttackRange(aiCharacter, nearestEnemy))
            {
                aiCharacter.Attack(nearestEnemy);
                yield return new WaitForSeconds(0.5f);
            }
            else if (chosenAction == "Defense")
            {
                Defense(aiCharacter);
                yield return new WaitForSeconds(0.5f);
            }
            else // Default to Move if chosen action is Move or can't perform Attack
            {
                HexTile targetTile = FindBestTileTowardsEnemy(aiCharacter, nearestEnemy);
                if (targetTile != null)
                {
                    var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, targetTile, GetMoveRange(aiCharacter));
                    if (path != null && path.Count > 0)
                    {
                        // Use MoveAlongPath for consistency
                        if (currentMovementCoroutine != null)
                        {
                            StopCoroutine(currentMovementCoroutine);
                        }

                        currentMovementCoroutine = StartCoroutine(MoveCharacterAlongPath(aiCharacter, path));

                        // Wait for movement to complete
                        yield return currentMovementCoroutine;
                    }
                    else
                    {
                        Debug.Log("No valid path found for AI movement");
                    }
                }
                else
                {
                    Debug.Log("No valid target tile found for AI movement");
                    // Default to defense if can't move
                    Defense(aiCharacter);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // Get new state after action
            string newState = GetStateRepresentation(aiCharacter, nearestEnemy);

            // Calculate reward (could be improved based on game state)
            float reward = CalculateReward(aiCharacter, nearestEnemy, chosenAction);

            // Update Q-table
            UpdateQTable(state, chosenAction, reward, newState);
        }
        else
        {
            Debug.Log("No enemies found for AI");
            yield return new WaitForSeconds(0.5f);
        }

        // End turn
        aiCharacter.EndTurn();

        // Final delay before ending turn
        yield return new WaitForSeconds(0.3f);
    }

    // Move character along path
    private IEnumerator MoveCharacterAlongPath(Character character, List<HexTile> path)
    {
        if (path == null || path.Count <= 1)
            yield break;

        // Skip the first tile (current position)
        for (int i = 1; i < path.Count; i++)
        {
            HexTile nextTile = path[i];

            // Move to the next tile
            character.MoveToTile(nextTile);

            // Reduce stamina for movement
            character.ReduceStamina(1);

            // Add delay for visual movement
            yield return new WaitForSeconds(0.3f);

            // Break if out of stamina
            if (character.currentStamina <= 0)
                break;
        }
    }

    // Create a state representation of the current game state
    private string GetStateRepresentation(Character aiCharacter, Character enemy)
    {
        int distanceToEnemy = 0;
        if (aiCharacter.currentTile != null && enemy.currentTile != null)
        {
            var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, enemy.currentTile);
            distanceToEnemy = path != null ? path.Count - 1 : 99;
        }

        // Format: "Health-Stamina-EnemyHealth-Distance"
        return $"{aiCharacter.currentHealth}-{aiCharacter.currentStamina}-{enemy.currentHealth}-{distanceToEnemy}";
    }

    // Calculate reward for reinforcement learning
    private float CalculateReward(Character aiCharacter, Character enemy, string action)
    {
        float reward = 0;

        if (action == "Attack")
        {
            reward += 10; // Base reward for attacking

            // Additional reward if enemy health is low
            if (enemy.currentHealth < 30)
                reward += 5;

            // Penalty if AI's stamina is critically low after attack
            if (aiCharacter.currentStamina <= 1)
                reward -= 2;
        }
        else if (action == "Defense")
        {
            reward += 5; // Base reward for defending

            // Additional reward if AI's health is low
            if (aiCharacter.currentHealth < 30)
                reward += 5;
        }
        else if (action == "Move")
        {
            // Reward based on whether AI is now in attack range
            if (IsInAttackRange(aiCharacter, enemy))
                reward += 8;
            else
            {
                var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, enemy.currentTile);
                int distanceToEnemy = path != null ? path.Count - 1 : 99;

                // Small reward for getting closer to enemy
                reward += 5 - Mathf.Min(5, distanceToEnemy);
            }
        }

        return reward;
    }

    // Find the nearest enemy using the pathfinding system
    private Character FindNearestEnemy(Character aiCharacter)
    {
        if (hexGrid == null)
        {
            hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null)
            {
                Debug.LogError("HexGrid reference is missing!");
                return null;
            }
        }

        var enemies = FindObjectsOfType<Character>()
            .Where(c => c.team != aiCharacter.team && c.gameObject.activeSelf && c.IsAlive())
            .ToList();
        if (enemies.Count == 0)
            return null;

        Character nearestEnemy = null;
        int shortestPathLength = int.MaxValue;

        foreach (var enemy in enemies)
        {
            
            if (enemy.currentTile == null || aiCharacter.currentTile == null)
                continue;

            if (aiCharacter.currentTile == null)
            {
                Debug.LogError($"{aiCharacter.characterName} does not have a valid currentTile!");
            }
            if (enemy.currentTile == null)
            {
                Debug.LogError($"{enemy.characterName} does not have a valid currentTile!");
            }
            HexTile targetTile = FindNearestTileInAttackRange(enemy.currentTile, attackRange);
            Debug.Log($"targetTile found {targetTile.X} : {targetTile.Y}");
            var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, targetTile);
            Debug.Log($"Path found from {aiCharacter.characterName} to {enemy.characterName}: {path} tiles");
            // If there's a path and it's shorter than the current shortest
            if (path != null && path.Count < shortestPathLength)
            {

                nearestEnemy = enemy;
                shortestPathLength = path.Count;
            }
        }
        //Debug.Log($"Nearest enemy found: {nearestEnemy?.characterName} at distance {shortestPathLength}");
        return nearestEnemy;
    }

    // Find the best tile to move towards an enemy
    private HexTile FindBestTileTowardsEnemy(Character aiCharacter, Character enemy)
    {
        if (enemy == null || enemy.currentTile == null || aiCharacter.currentTile == null)
            return null;

        // Find all reachable tiles within move range
        var reachableTiles = new List<HexTile>();
        int moveRange = GetMoveRange(aiCharacter);

        // Get grid dimensions
        var grid = hexGrid.AdjacentTilesGrid;
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                HexTile tile = grid[x, y];
                if (tile != null && !tile.IsOccupied && tile != aiCharacter.currentTile)
                {
                    var path = Pathfinding.FindPath(
                        grid,
                        aiCharacter.currentTile,
                        tile,
                        moveRange);

                    if (path != null && path.Count <= moveRange + 1 && PathIsWithinStaminaRange(aiCharacter, path))
                    {
                        reachableTiles.Add(tile);
                    }
                }
            }
        }

        // If no reachable tiles, return null
        if (reachableTiles.Count == 0)
            return null;

        // Determine if we should move towards or away from enemy based on strategy
        bool moveTowardsEnemy = ShouldMoveTowardsEnemy(aiCharacter, enemy);

        HexTile bestTile = null;
        int bestDistance = moveTowardsEnemy ? int.MaxValue : 0;

        foreach (var tile in reachableTiles)
        {
            var pathToEnemy = Pathfinding.FindPath(grid, tile, enemy.currentTile);
            if (pathToEnemy != null)
            {
                int distance = pathToEnemy.Count - 1; // Don't count starting tile

                if (moveTowardsEnemy)
                {
                    // Find tile that gets us closest to enemy
                    if (distance < bestDistance)
                    {
                        bestTile = tile;
                        bestDistance = distance;
                    }

                    // If we can get in attack range, prioritize that tile
                    if (distance <= attackRange)
                    {
                        bestTile = tile;
                        break;
                    }
                }
                else
                {
                    // Find tile that keeps us at a safe distance
                    if (distance > bestDistance && distance > attackRange)
                    {
                        bestTile = tile;
                        bestDistance = distance;
                    }
                }
            }
        }

        return bestTile;
    }

    // Helper method to get move range based on stamina
    private int GetMoveRange(Character character)
    {
        return character.currentStamina;
    }

    // Decide whether to move towards or away from enemy based on current situation
    private bool ShouldMoveTowardsEnemy(Character aiCharacter, Character enemy)
    {
        // Default to moving towards enemy
        bool moveTowardsEnemy = true;

        // If health is low and enemy is stronger, consider retreating
        if (aiCharacter.currentHealth < enemy.currentHealth && aiCharacter.currentHealth < 30)
        {
            moveTowardsEnemy = false;
        }

        // If low on stamina, might want to keep distance
        if (aiCharacter.currentStamina <= 1)
        {
            moveTowardsEnemy = false;
        }

        // Higher difficulty AI is more aggressive
        if (aiDifficulty >= 4)
        {
            moveTowardsEnemy = true;
        }

        return moveTowardsEnemy;
    }

    // Check if a path is within the character's stamina range
    private bool PathIsWithinStaminaRange(Character character, List<HexTile> path)
    {
        int pathLength = path.Count > 0 && path[0] == character.currentTile
            ? path.Count - 1
            : path.Count;

        return pathLength <= character.currentStamina;
    }

    // Check if target is within attack range
    private bool IsInAttackRange(Character attacker, Character target)
    {
        if (attacker == null || target == null || attacker.currentTile == null || target.currentTile == null)
            return false;

        var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, attacker.currentTile, target.currentTile);

        // Check if distance is within attack range (path.Count - 1 because we don't count current tile)
        return path != null && path.Count - 1 <= attackRange;
    }

    // Perform defense action
    private void Defense(Character character)
    {
        Debug.Log($"{character.characterName} takes defensive stance");

        // We need to implement defense mechanics
        // For now, we'll simply reduce stamina
        character.ReduceStamina(1);
    }
    //phần tìm ô gần nhất trong tầm đánh cần tích hợp A* từ pathFinding để tìm ô tối ưu nhất gần ô enemyTile nhất
    private HexTile FindNearestTileInAttackRange(HexTile enemyTile, int attackRange) 
    {
        if (enemyTile == null)
            return null;

        HexTile nearestTile = null;
        int shortestDistance = int.MaxValue;

        foreach (var tile in hexGrid.Tiles)
        {
            if (tile != null && !tile.IsOccupied)
            {
                // Tính khoảng cách bằng số ô (Manhattan distance trên lưới hex)
                int distance = CalculateHexDistance(tile, enemyTile);

                // Kiểm tra nếu tile nằm trong tầm đánh
                if (distance <= attackRange && distance < shortestDistance)
                {
                    nearestTile = tile;
                    shortestDistance = distance;
                }
            }
        }

        return nearestTile;
    }

    // Hàm tính khoảng cách giữa hai ô trên lưới hex
    private int CalculateHexDistance(HexTile a, HexTile b)
    {
        return (Mathf.Abs(a.X - b.X)
              + Mathf.Abs(a.Y - b.Y)
              + Mathf.Abs((-a.X - a.Y) - (-b.X - b.Y))) / 2;
    }
}
/*
// Action data class used by the AI
[System.Serializable]
public class ActionData
{
    public string ActionName;
    public int Damage;
    public int Stamina;
    public int PositionAdvantage;

    public ActionData(string actionName, int damage, int stamina, int positionAdvantage)
    {
        ActionName = actionName;
        Damage = damage;
        Stamina = stamina;
        PositionAdvantage = positionAdvantage;
    }
}
//chuyển list<Tiles> thành 2D array
HexTile[,] adjacentGrid = ConvertToGrid(attacker.currentTile.AdjacentTiles);
var path = Pathfinding.FindPath(adjacentGrid, attacker.currentTile, target.currentTile);
private HexTile[,] ConvertToGrid(List<HexTile> tiles)
{
    int size = tiles.Count;
    HexTile[,] grid = new HexTile[size, size];

    for (int i = 0; i < size; i++)
    {
        grid[i, 0] = tiles[i];
    }

    return grid;
}

*/