using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameOver : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button retry;
    public Button mainMenu;
    public float fadeInTimer;
    public EventSystem events;
    public RectTransform skullTransform;

    void Start()
    {
        canvasGroup.alpha = 0;
        retry.enabled = false;
        mainMenu.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        canvasGroup.alpha += fadeInTimer * Time.deltaTime;


        if(canvasGroup.alpha == 1)
        {
            retry.enabled = true;
            mainMenu.enabled = true;
            float skullPosition = events.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition.y;
            skullTransform.anchoredPosition = new Vector2(skullTransform.anchoredPosition.x, skullPosition);
        }
        
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
