using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPauseMenu : MonoBehaviour
{
    public Animator animator;
    public GameObject pauseMenu;
    public GameObject closeBtn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void ShowCloseBtn()
    {
        closeBtn.SetActive(true);
    }

    public void HideCloseBtn()
    {
        closeBtn.SetActive(false);
    }

    public void LoadAnimation()
    {
        animator.SetBool("IsPaused", false);
    }
}
