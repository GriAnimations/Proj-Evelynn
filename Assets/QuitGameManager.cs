using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class QuitGameManager : MonoBehaviour
{
    [SerializeField] private GameObject quitGameUI;
    [SerializeField] private GameObject bgImage;

    public bool quitGDisplayed;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideQuitGameUI();
        }
    }

    public void DisplayQuitGameUI()
    {
        quitGameUI.SetActive(true);
    }

    public void HideQuitGameUI()
    {
        bgImage.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        bgImage.SetActive(false);
        quitGameUI.SetActive(false);
        quitGDisplayed = false;
        
        gameObject.SetActive(false);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void FadeBlackStart()
    {
        quitGDisplayed = true;
        bgImage.SetActive(true);
        StartCoroutine(FadeBlack());
    }

    private IEnumerator FadeBlack()
    {
        var elapsedTime = 0f;
        while (elapsedTime <= 1f)
        {
            if (!quitGDisplayed)
            {
                break;
            }
            
            elapsedTime += Time.deltaTime;
            
            var normalizedTime = Mathf.Clamp01(elapsedTime / 1f);
            
            var alpha = Mathf.Lerp(0f, 0.9f, normalizedTime);
            
            bgImage.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
            
            yield return null;
        }
    }
}
