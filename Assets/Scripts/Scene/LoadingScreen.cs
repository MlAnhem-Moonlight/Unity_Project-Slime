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
    private float originalTimeScale = 1f; // To store the original time scale

    private void Start()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false); // Hide the loading screen at the start
    }

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
        // Freeze the game by setting time scale to 0 (except the loading process)
        originalTimeScale = Time.timeScale; // Save the current time scale
        Time.timeScale = 0; // Freeze all gameplay elements

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

            // Check if loading is complete
            if (operation.progress >= 0.9f && !isSceneActivationTriggered)
            {
                text.SetText("Press Space to activate the scene");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isSceneActivationTriggered = true;
                    operation.allowSceneActivation = true; // Activate the scene
                }
            }

            yield return null; // Keep the loading process active
        }

        // Restore the original time scale and hide the loading screen
        Time.timeScale = originalTimeScale; // Unfreeze gameplay
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}
