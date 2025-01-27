using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private GameObject questionImage;
    [SerializeField] private GameObject escapeButton;
    
    public bool questionsDisplayed;

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
        //add here the off image
        
        questionsDisplayed = false;
    }

    public void SpawnQuestions()
    {
        if (!questionsDisplayed)
        {
            questionImage.SetActive(true);
            escapeButton.SetActive(true);
            //add here the on image
            //the button as anim event after the question spawn anim is done
            
            questionsDisplayed = true;
        }
        else
        {
            DespawnQuestions();
        }
    }
}
