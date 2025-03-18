using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<Character> characters; // Danh sách nhân vật
    private int currentCharacterIndex = 0; // Vị trí nhân vật hiện tại

    void Start()
    {
        // Khởi tạo lượt cho nhân vật đầu tiên
        if (characters.Count > 0)
        {
            StartTurn();
        }
    }

    void StartTurn()
    {
        Debug.Log($"Turn started for: {characters[currentCharacterIndex].name}");
        characters[currentCharacterIndex].StartTurn();
    }

    public void EndTurn()
    {
        // Kết thúc lượt của nhân vật hiện tại
        characters[currentCharacterIndex].EndTurn();

        // Chuyển sang nhân vật tiếp theo
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;

        // Bắt đầu lượt mới
        StartTurn();
    }

    void CheckGameOver()
    {
        int aliveCharacters = 0;
        foreach (var character in characters)
        {
            if (character.health > 0)
            {
                aliveCharacters++;
            }
        }

        if (aliveCharacters <= 1)
        {
            Debug.Log("Game Over!");
        }
    }

}
