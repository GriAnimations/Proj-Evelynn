using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private GameObject questionImage;
    [SerializeField] private GameObject escapeButton;
    [SerializeField] private Image questionLight;
    
    public bool questionsDisplayed;


    private void Start()
    {
        questionLight.color = new Color(0.5f, 0.18f, 0.3f);
    }

    private void Update()
    {
        if (questionsDisplayed && Input.GetKeyDown(KeyCode.Escape))
        {
            DespawnQuestions();
        }
    }

    public void DespawnQuestions()
    {
        //replace with play anim into anim event
        //also set button to non-interactive on button press
        
        questionImage.SetActive(false);
        escapeButton.SetActive(false);
        questionLight.color = new Color(0.5f, 0.18f, 0.3f);
        
        questionsDisplayed = false;
    }

    public void SpawnQuestions()
    {
        if (!questionsDisplayed)
        {
            questionImage.SetActive(true);
            escapeButton.SetActive(true);
            questionLight.color = new Color(0f, 0.88f, 0.95f);
            //the button as anim event after the question spawn anim is done
            
            questionsDisplayed = true;
        }
        else
        {
            DespawnQuestions();
        }
    }
}
