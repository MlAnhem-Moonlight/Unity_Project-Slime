using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInGameEvent : MonoBehaviour
{
    public GameObject pauseMenu;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        animator.SetBool("IsPaused", true);
    }
}
