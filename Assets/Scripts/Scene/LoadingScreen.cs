using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;

// Explicitly alias namespaces for disambiguation
using UI = UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public UI.Slider progressBar; // Use Slider from UnityEngine.UI
    public SceneAsset gameScene;
    public UI.Image hint; // Use Image from UnityEngine.UI
    public TMPro.TMP_Text text; 

    private bool isSceneActivationTriggered = false; // To track user input

    public void LoadScene()
    {
        StartCoroutine(LoadAsynchronously(gameScene.name));
    }

    public void LoadScene(SceneAsset scene)
    {
        StartCoroutine(LoadAsynchronously(scene.name));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent automatic scene activation

        if (loadingScreen != null)
            loadingScreen.SetActive(true); // Show the loading screen

        while (!operation.isDone)
        {
            // Calculate progress manually
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Normalize progress between 0 and 0.9
            if (progressBar != null)
                progressBar.value = progress;

            Debug.Log($"Progress: {progress * 100}%");

            // Check if loading is complete
            if (operation.progress >= 0.9f && !isSceneActivationTriggered)
            {
                // Scene is ready but waiting for player input
                if (hint != null)
                    hint.enabled = true;

                text.SetText("Press Space to activate the scene");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isSceneActivationTriggered = true;
                    operation.allowSceneActivation = true; // Activate the scene
                }
            }

            yield return null;
        }

        // Hide the loading screen after activation
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}
