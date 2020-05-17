using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Canvas pauseCanvas;
    public EventSystem events;
    public RectTransform skullTransform;
    public bool paused;

    void Start()
    {
        pauseCanvas = GetComponent<Canvas>();
        pauseCanvas.enabled = true;
        paused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            if (!paused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        if (pauseCanvas.enabled)
        {
            float skullPosition = events.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition.y;
            skullTransform.anchoredPosition = new Vector2(skullTransform.anchoredPosition.x, skullPosition);
        }

    }


    public void PauseGame()
    {
        pauseCanvas.enabled = true;
        Time.timeScale = 0;
        paused = true;
    }

    public void ResumeGame()
    {

        pauseCanvas.enabled = false;
        Time.timeScale = 1;
        paused = false;
    }

    public void ReloadLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

}
