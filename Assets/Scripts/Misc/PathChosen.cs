using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PathChosen : MonoBehaviour
{
    public int chosenPath;
    public GameObject loadTriggerObject;
    public SceneAsset state2_1;
    public SceneAsset state2_2;

    // Start is called before the first frame update
    void Start()
    {
        LoadSave();
        ChoosePath();
    }

    void LoadSave()
    {
        string filePath = "./Assets/SaveTest/Path.txt"; 
        try
        {
            chosenPath = ReadPathValueFromFile(filePath);

            if (chosenPath != -1)
            {
                Debug.Log("Giá trị đọc từ file: " + chosenPath);
            }
            else
            {
                Debug.Log("Không tìm thấy giá trị hợp lệ trong file.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading file: {ex.Message}");
        }
    }

    static int ReadPathValueFromFile(string filePath)
    {
        try
        {
            // Đọc tất cả các dòng từ file
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("Path: "))
                {
                    string valueString = line.Substring("Path: ".Length);
                    if (int.TryParse(valueString, out int value))
                    {
                        return value; // Trả về giá trị số sau "Path: "
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
        }

        return -1; // Trả về -1 nếu không tìm thấy giá trị hợp lệ
    }

    void ChoosePath()
    {
        switch (chosenPath)
            {
                case 1:
                    Debug.Log("Path 1(Hero) chosen");
                    // Add logic for path 1
                    LoadScreen(state2_1);
                    ChangeTag.ChangeLayerToTag("Human", "Ally");
                    ChangeTag.ChangeLayerToTag("Unknow", "Neutral");
                    break;
                case 2:
                    Debug.Log("Path 2(Knight) chosen");
                    // Add logic for path 2
                    LoadScreen(state2_2);
                    ChangeTag.ChangeLayerToTag("Human", "Neutral");
                    ChangeTag.ChangeLayerToTag("Unknow", "Neutral");
                    break;
                case 3:
                    Debug.Log("Path 3(SmolMonster) chosen");
                    // Add logic for path 3
                    LoadScreen(state2_2);
                    ChangeTag.ChangeLayerToTag("Unknow", "Neutral");
                    ChangeTag.ChangeLayerToTag("Demon", "Neutral");
                    break;
                case 4:
                    Debug.Log("Path 4(DemonKing) chosen");
                    // Add logic for path 4
                    LoadScreen(state2_1);
                    ChangeTag.ChangeLayerToTag("Demon", "Ally");
                    break;
                default:
                    Debug.Log("No path chosen");
                    break;
            }

    }

    private void LoadScreen(SceneAsset scene)
    {
        try
        {
            Debug.Log("Loading scene: " + scene.name);
            loadTriggerObject.GetComponent<LoadingScreen>().LoadScene(scene); 
        }
        catch(Exception ex)
        {
            Debug.Log($"Error loading scene: {ex.Message}");
        }
    }
}
