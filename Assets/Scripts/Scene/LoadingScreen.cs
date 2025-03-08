using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider progressBar;
    public SceneAsset gameScene;
    public Image hint;

    private bool isSceneActivationTriggered = false;

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
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            Debug.Log(operation.progress);
            if (operation.progress >= 0.9f)
            {
                progressBar.gameObject.SetActive(false);
                hint.gameObject.SetActive(true);
                

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isSceneActivationTriggered = true;
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        loadingScreen.SetActive(false);
    }
}
