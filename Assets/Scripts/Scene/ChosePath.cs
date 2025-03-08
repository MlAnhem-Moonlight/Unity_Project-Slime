using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChosePath : MonoBehaviour
{
    public int Path = 3;
    public GameObject holySword;
    public GameObject hero;
    public GameObject demonKing;

    public SceneAsset state2_1;
    public SceneAsset state2_2;

    private bool isChoosePath = false;

    private Dictionary<Func<bool>, Action> pathActions;

    void Start()
    {
        pathActions = new Dictionary<Func<bool>, Action>
        {
            { () => holySword == null && hero == null, () => { SetPath(1); LockDemonKingPath(); } },
            { () => holySword == null && hero != null, () => { SetPath(2); LockDemonKingPath(); } },
            { () => holySword != null && hero == null, () => { SetPath(3); LockDemonKingPath(); } },
            { () => demonKing == null, () => { SetPath(4); LockHeroAndHolySword(); } }
        };
    }

    void Update()
    {
        if (isChoosePath) return;

        foreach (var pathAction in pathActions)
        {
            if (pathAction.Key())
            {
                pathAction.Value();
                break;
            }
        }
    }

    private void SetPath(int path)
    {
        Path = path;
        Debug.Log($"Path: {Path} has been chosen");
        isChoosePath = true;
    }

    private void LockDemonKingPath()
    {
        DisableComponents(demonKing, "DemonKing");
    }

    private void LockHeroAndHolySword()
    {
        DisableComponents(hero, "Hero");
        DisableComponents(holySword, "HolySword");
    }

    private void DisableComponents(GameObject obj, string objName)
    {
        try
        {
            obj.GetComponent<ActivateButton>().enabled = false;
            obj.GetComponent<DestroyGameObject>().enabled = false;
            obj.GetComponent<Collider2D>().isTrigger = true;
            obj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
        catch (Exception e)
        {
            Debug.Log($"{objName}: {e.Message}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ChoosePath1(Path);
        }
    }

    private void ChoosePath(int path)
    {
        string filePath = "./Assets/SaveTest/Path.txt";
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Scene scene = SceneManager.GetActiveScene();
        try
        { 
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Path: " + path);
                writer.WriteLine(scene.name+ ": " + player.transform.position);
            }
            Debug.Log("File written successfully.");
        }
        catch (Exception ex)
        {
            Debug.Log($"Error writing file: {ex.Message}");
        }
    }

    void ChoosePath1(int chosenPath)
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
            GetComponent<LoadingScreen>().LoadScene(scene);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading scene: {ex.Message}");
        }
    }
}
