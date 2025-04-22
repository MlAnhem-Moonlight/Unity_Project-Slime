using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // References
    private HexGrid hexGrid;
    private TurnManager turnManager;

    // Q-Learning parameters
    private Dictionary<(string, string), float> qTable = new Dictionary<(string, string), float>();
    private float learningRate = 0.1f;
    private float discountFactor = 0.9f;
    private float explorationRate = 0.2f;

    // DDA parameters
    private int aiDifficulty = 1;
    private int playerWinCount = 0;
    private int playerLossCount = 0;
    private float baseExplorationRate = 0.3f;

    [SerializeField] private List<ActionData> availableActions;
    private Coroutine currentMovementCoroutine;

    [Tooltip("Attack range of AI characters")]
    public int attackRange = 1;
    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        hexGrid = FindObjectOfType<HexGrid>();
        InitializeActions();
    }

    private void InitializeActions()
    {
        availableActions = new List<ActionData>
                {
                    new ActionData("AggressiveAdvance", damage: 20, stamina: 2, positionAdvantage: 2),
                    new ActionData("DefensiveRetreat", damage: 0, stamina: 1, positionAdvantage: 3),
                    new ActionData("FlankingMove", damage: 15, stamina: 2, positionAdvantage: 2),
                    new ActionData("StandGround", damage: 10, stamina: 1, positionAdvantage: 1),
                    new ActionData("FullAttack", damage: 25, stamina: 3, positionAdvantage: 0)
                };
    }

    public IEnumerator HandleAITurn(Character aiCharacter)
    {
        attackRange = aiCharacter.AtkRange;
        yield return new WaitForSeconds(0.5f);
        aiCharacter.StartTurn();

        Character nearestEnemy = FindNearestEnemy(aiCharacter);
        Debug.Log($"Nearest enemy: {nearestEnemy?.characterName}");
        if (nearestEnemy == null)
        {
            aiCharacter.EndTurn();
            yield break;
        }

        string currentState = GetStateRepresentation(aiCharacter, nearestEnemy);
        string chosenAction = ChooseBestAction(currentState, aiCharacter);
        yield return StartCoroutine(ExecuteAction(chosenAction, aiCharacter, nearestEnemy));

        string newState = GetStateRepresentation(aiCharacter, nearestEnemy);
        float reward = CalculateReward(aiCharacter, nearestEnemy, chosenAction);
        UpdateQTable(currentState, chosenAction, reward, newState);

        aiCharacter.EndTurn();
        yield return new WaitForSeconds(0.3f);
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
    private string ChooseBestAction(string state, Character aiCharacter)
    {
        // Exploration vs Exploitation
        if (Random.value < GetAdjustedExplorationRate())
        {
            return GetRandomViableAction(aiCharacter);
        }

        return GetOptimalAction(state, aiCharacter);
    }

    private float GetAdjustedExplorationRate()
    {
        return Mathf.Max(0.1f, baseExplorationRate - (aiDifficulty * 0.05f));
    }

    private string GetRandomViableAction(Character aiCharacter)
    {
        var viableActions = availableActions.Where(a => a.Stamina <= aiCharacter.currentStamina).ToList();
        return viableActions[Random.Range(0, viableActions.Count)].ActionName;
    }

    private string GetOptimalAction(string state, Character aiCharacter)
    {
        Dictionary<string, float> actionScores = new Dictionary<string, float>();
        foreach (var action in availableActions)
        {
            if (action.Stamina <= aiCharacter.currentStamina)
            {
                float qValue = qTable.ContainsKey((state, action.ActionName)) ? qTable[(state, action.ActionName)] : 0;
                float utilityScore = EvaluateAction(action, aiCharacter);
                actionScores[action.ActionName] = (qValue * 0.7f) + (utilityScore * 0.3f);
            }
        }

        return actionScores.Count > 0 ?
            actionScores.OrderByDescending(kvp => kvp.Value).First().Key :
            "StandGround";
    }

    private float EvaluateAction(ActionData action, Character aiCharacter)
    {
        float healthWeight = 1.0f + (aiDifficulty * 0.2f);
        float staminaWeight = 0.5f + (aiDifficulty * 0.1f);
        float positionWeight = 0.8f + (aiDifficulty * 0.15f);

        float healthRatio = (float)aiCharacter.currentHealth / aiCharacter.maxHealth;
        float staminaRatio = (float)aiCharacter.currentStamina / aiCharacter.maxStamina;

        return (action.Damage * healthWeight * (1 - healthRatio)) +
               (action.PositionAdvantage * positionWeight) -
               (action.Stamina * staminaWeight * (1 - staminaRatio));
    }

    private IEnumerator ExecuteAction(string actionName, Character aiCharacter, Character enemy)
    {
        switch (actionName)
        {
            case "AggressiveAdvance":
                yield return StartCoroutine(ExecuteAggressiveAdvance(aiCharacter, enemy));
                break;
            case "DefensiveRetreat":
                yield return StartCoroutine(ExecuteDefensiveRetreat(aiCharacter, enemy));
                break;
            case "FlankingMove":
                yield return StartCoroutine(ExecuteFlankingMove(aiCharacter, enemy));
                break;
            case "StandGround":
                yield return StartCoroutine(ExecuteStandGround(aiCharacter));
                break;
            case "FullAttack":
                yield return StartCoroutine(ExecuteFullAttack(aiCharacter, enemy));
                break;
        }
    }

    private IEnumerator ExecuteAggressiveAdvance(Character aiCharacter, Character enemy)
    {
        // Move towards enemy aggressively
        HexTile targetTile = FindBestTileTowardsEnemy(aiCharacter, enemy);
        if (targetTile != null)
        {
            var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, targetTile);
            if (path != null)
            {
                yield return StartCoroutine(MoveCharacterAlongPath(aiCharacter, path));
            }
        }

        // Attack if in range
        if (IsInAttackRange(aiCharacter, enemy) && aiCharacter.currentStamina >= 2)
        {
            aiCharacter.Attack(enemy);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ExecuteDefensiveRetreat(Character aiCharacter, Character enemy)
    {
        // Find safe tile away from enemy
        HexTile safeTile = FindBestTileTowardsEnemy(aiCharacter, enemy);
        if (safeTile != null)
        {
            var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, safeTile);
            if (path != null)
            {
                yield return StartCoroutine(MoveCharacterAlongPath(aiCharacter, path));
            }
        }
    }

    private IEnumerator ExecuteFlankingMove(Character aiCharacter, Character enemy)
    {
        // Find flanking position
        HexTile flankTile = FindFlankingTile(aiCharacter, enemy);
        if (flankTile != null)
        {
            var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, flankTile);
            if (path != null)
            {
                yield return StartCoroutine(MoveCharacterAlongPath(aiCharacter, path));
                if (IsInAttackRange(aiCharacter, enemy) && aiCharacter.currentStamina >= 1)
                {
                    aiCharacter.Attack(enemy);
                }
            }
        }
    }

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

    private bool PathIsWithinStaminaRange(Character character, List<HexTile> path)
    {
        int pathLength = path.Count > 0 && path[0] == character.currentTile
            ? path.Count - 1
            : path.Count;

        return pathLength <= character.currentStamina;
    }

    private int GetMoveRange(Character character)
    {
        return character.currentStamina;
    }
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

    private IEnumerator ExecuteStandGround(Character aiCharacter)
    {
        // Defensive stance, maybe implement defense bonus later
        aiCharacter.ReduceStamina(1);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ExecuteFullAttack(Character aiCharacter, Character enemy)
    {
        if (IsInAttackRange(aiCharacter, enemy) && aiCharacter.currentStamina >= 3)
        {
            // Powerful attack implementation
            aiCharacter.Attack(enemy);
            aiCharacter.Attack(enemy);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // Move to attack range if possible
            HexTile attackTile = FindNearestTileInAttackRange(enemy.currentTile, aiCharacter.AtkRange);
            if (attackTile != null)
            {
                var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, attackTile);
                if (path != null)
                {
                    yield return StartCoroutine(MoveCharacterAlongPath(aiCharacter, path));
                }
            }
        }
    }

    private HexTile FindNearestTileInAttackRange(HexTile enemyTile, int attackRange)
    {
        if (enemyTile == null)
        {
            Debug.LogError("Enemy tile is null!");
            return null;
        }

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
    private Character FindNearestEnemy(Character aiCharacter)
    {
        var enemies = turnManager.allCharacters
            .Where(c => c.team != aiCharacter.team && c.IsAlive())
            .ToList();
        Debug.Log($"Enemies found: {enemies.Count}");
        if (enemies.Count == 0)
            return null;

        Character nearestEnemy = null;
        int shortestPathLength = int.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy.currentTile == null || aiCharacter.currentTile == null)
                continue;

            var path = Pathfinding.FindingPathOccupied(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, enemy.currentTile);
            Debug.Log($"Path found to {enemy.characterName}: {path?.Count} tiles");
            if (path != null && path.Count < shortestPathLength)
            {
                nearestEnemy = enemy;
                shortestPathLength = path.Count;
            }
            else Debug.Log($"No path found to {enemy.characterName}");
        }

        return nearestEnemy;
    }
    private string GetStateRepresentation(Character aiCharacter, Character enemy)
    {
        int distanceToEnemy = CalculateHexDistance(aiCharacter.currentTile, enemy.currentTile);
        string healthState = GetHealthState(aiCharacter.currentHealth);
        string staminaState = GetStaminaState(aiCharacter.currentStamina);
        string enemyHealthState = GetHealthState(enemy.currentHealth);
        string positionState = GetPositionState(aiCharacter, enemy);

        return $"{healthState}-{staminaState}-{enemyHealthState}-{distanceToEnemy}-{positionState}";
    }

    private float CalculateReward(Character aiCharacter, Character enemy, string action)
    {
        float reward = 0;
        int newDistance = CalculateHexDistance(aiCharacter.currentTile, enemy.currentTile);
        bool inAttackRange = IsInAttackRange(aiCharacter, enemy);

        // Base rewards based on action type and situation
        switch (action)
        {
            case "AggressiveAdvance":
                reward += inAttackRange ? 15 : 5;
                reward += aiCharacter.currentHealth > enemy.currentHealth ? 5 : -5;
                break;

            case "DefensiveRetreat":
                reward += aiCharacter.currentHealth < enemy.currentHealth ? 10 : 0;
                reward += aiCharacter.currentStamina < 2 ? 5 : -5;
                break;

            case "FlankingMove":
                reward += inAttackRange ? 10 : 0;
                reward += IsInAdvantageousPosition(aiCharacter, enemy) ? 8 : -3;
                break;

            case "StandGround":
                reward += aiCharacter.currentStamina < 2 ? 5 : -5;
                reward += inAttackRange && aiCharacter.currentHealth > enemy.currentHealth ? 5 : 0;
                break;

            case "FullAttack":
                reward += inAttackRange ? 20 : -10;
                reward += aiCharacter.currentStamina >= 3 ? 5 : -10;
                break;
        }

        // Additional situational rewards
        reward += aiCharacter.currentHealth > enemy.currentHealth ? 5 : -5;
        reward += inAttackRange && aiCharacter.currentStamina >= 2 ? 10 : 0;

        return reward;
    }

    private int CalculateHexDistance(HexTile a, HexTile b)
    {
        return (Mathf.Abs(a.X - b.X)
              + Mathf.Abs(a.Y - b.Y)
              + Mathf.Abs((-a.X - a.Y) - (-b.X - b.Y))) / 2;
    }
    private bool IsInAdvantageousPosition(Character aiCharacter, Character enemy)
    {
        if (enemy.currentTile == null || aiCharacter.currentTile == null)
            return false;

        // Check if we're flanking the enemy
        var enemyAdjacentTiles = enemy.currentTile.AdjacentTiles;
        int occupiedAdjacentTiles = enemyAdjacentTiles.Count(t => t.IsOccupied);

        return occupiedAdjacentTiles <= 2 && IsInAttackRange(aiCharacter, enemy);
    }

    public void AdjustDifficulty(bool playerWon)
    {
        if (playerWon)
            playerWinCount++;
        else
            playerLossCount++;

        if (playerWinCount >= 3)
        {
            aiDifficulty = Mathf.Min(5, aiDifficulty + 1);
            playerWinCount = 0;
            explorationRate *= 0.9f;
        }
        else if (playerLossCount >= 3)
        {
            aiDifficulty = Mathf.Max(1, aiDifficulty - 1);
            playerLossCount = 0;
            explorationRate *= 1.1f;
        }

        explorationRate = Mathf.Clamp(explorationRate, 0.1f, 0.4f);
        Debug.Log($"AI Difficulty adjusted to: {aiDifficulty}, Exploration Rate: {explorationRate:F2}");
    }

    // Helper methods
    private string GetHealthState(int health) =>
        health > 70 ? "High" : health > 30 ? "Medium" : "Low";

    private string GetStaminaState(int stamina) =>
        stamina >= 4 ? "High" : stamina >= 2 ? "Medium" : "Low";

    private string GetPositionState(Character aiCharacter, Character enemy)
    {
        if (IsInAttackRange(aiCharacter, enemy))
            return "AttackRange";
        if (IsInAdvantageousPosition(aiCharacter, enemy))
            return "Advantageous";
        return "Normal";
    }


    private bool IsInAttackRange(Character attacker, Character target)
    {
        if (attacker == null || target == null || attacker.currentTile == null || target.currentTile == null)
            return false;

        var path = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, attacker.currentTile, target.currentTile);

        return path != null && path.Count - 1 <= attackRange;
    }
    private HexTile FindFlankingTile(Character aiCharacter, Character enemy)
    {
        if (enemy == null || enemy.currentTile == null || aiCharacter.currentTile == null)
            return null;

        // Get all tiles within movement range
        var reachableTiles = new List<HexTile>();
        int moveRange = GetMoveRange(aiCharacter);

        // Get enemy adjacent tiles for flanking analysis
        var enemyAdjacentTiles = enemy.currentTile.AdjacentTiles;
        var occupiedAdjacentTiles = enemyAdjacentTiles.Where(t => t.IsOccupied).ToList();

        // Get potential flanking positions
        var flankingPositions = new List<(HexTile tile, float score)>();

        foreach (var tile in hexGrid.Tiles)
        {
            if (tile != null && !tile.IsOccupied && tile != aiCharacter.currentTile)
            {
                // Check if tile is reachable within stamina
                var pathToTile = Pathfinding.FindPath(
                    hexGrid.AdjacentTilesGrid,
                    aiCharacter.currentTile,
                    tile,
                    moveRange);

                if (pathToTile != null && pathToTile.Count <= moveRange + 1 &&
                    PathIsWithinStaminaRange(aiCharacter, pathToTile))
                {
                    // Calculate flanking score for this tile
                    float flankingScore = CalculateFlankingScore(tile, enemy, aiCharacter, occupiedAdjacentTiles);

                    if (flankingScore > 0)
                    {
                        flankingPositions.Add((tile, flankingScore));
                    }
                }
            }
        }

        // Return the tile with highest flanking score
        if (flankingPositions.Count > 0)
        {
            var bestPosition = flankingPositions.OrderByDescending(p => p.score).First();
            return bestPosition.tile;
        }

        // If no good flanking position found, return null
        return null;
    }

    private float CalculateFlankingScore(HexTile tile, Character enemy, Character aiCharacter, List<HexTile> occupiedEnemyAdjacent)
    {
        float score = 0;

        // Base score calculations
        int distanceToEnemy = CalculateHexDistance(tile, enemy.currentTile);

        // Must be within attack range
        if (distanceToEnemy > attackRange)
            return 0;

        // Calculate angle between AI and occupied tiles relative to enemy
        foreach (var occupiedTile in occupiedEnemyAdjacent)
        {
            float angle = CalculateAngleBetweenTiles(enemy.currentTile, occupiedTile, tile);

            // Higher score for positions opposite to occupied tiles (ideal flanking angle ~180 degrees)
            if (angle > 120f)
                score += 5f;
            else if (angle > 90f)
                score += 3f;
        }

        // Bonus for positions that minimize enemy escape routes
        var enemyEscapeRoutes = enemy.currentTile.AdjacentTiles.Count(t => !t.IsOccupied);
        score += (6 - enemyEscapeRoutes) * 2;

        // Factor in AI difficulty
        score *= (1f + (aiDifficulty * 0.1f));

        // Penalty for tiles too close to other enemies
        foreach (var otherChar in turnManager.allCharacters)
        {
            if (otherChar != enemy && otherChar != aiCharacter && otherChar.team != aiCharacter.team)
            {
                int distanceToOther = CalculateHexDistance(tile, otherChar.currentTile);
                if (distanceToOther <= 2)
                    score -= (2 - distanceToOther) * 3;
            }
        }

        // Consider stamina cost
        var pathToTile = Pathfinding.FindPath(hexGrid.AdjacentTilesGrid, aiCharacter.currentTile, tile);
        if (pathToTile != null)
        {
            int staminaCost = pathToTile.Count - 1;
            score -= staminaCost * 0.5f;
        }

        return Mathf.Max(0, score);
    }

    private float CalculateAngleBetweenTiles(HexTile center, HexTile tile1, HexTile tile2)
    {
        // Convert hex coordinates to world position vectors
        Vector2 centerPos = new Vector2(center.X, center.Y);
        Vector2 pos1 = new Vector2(tile1.X, tile1.Y);
        Vector2 pos2 = new Vector2(tile2.X, tile2.Y);

        // Calculate vectors from center to each tile
        Vector2 vector1 = pos1 - centerPos;
        Vector2 vector2 = pos2 - centerPos;

        // Calculate angle between vectors
        float angle = Vector2.Angle(vector1, vector2);

        return angle;
    }

}
