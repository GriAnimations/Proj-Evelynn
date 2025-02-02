using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

public class QuestionManager : MonoBehaviour
{
    private static readonly int Status = Animator.StringToHash("status");
    [SerializeField] private GameObject questionImage;
    [SerializeField] private GameObject escapeButton;
    [SerializeField] private Animator animator;
    [SerializeField] private QuitGameManager quitGameManager;
    [SerializeField] private SoundManager soundManager;
    
    public bool questionsDisplayed;
    

    private void Update()
    {
        if (questionsDisplayed && Input.GetKeyDown(KeyCode.Escape) && !quitGameManager.quitGDisplayed)
        {
            ToggleQuestions();
        }
    }

    public void ToggleQuestions()
    {
        if (!questionsDisplayed)
        {
            SpawnQuestion();
        }
        else
        {
            DespawnQuestions();
        }
        
        questionsDisplayed = !questionsDisplayed;
    }
    
    private void SpawnQuestion()
    {
        animator.Play("UI Animation");
        soundManager.PlayClickSound(6);
    }

    public void DisplayUI()
    {
        questionImage.SetActive(true);
        escapeButton.SetActive(true);
    }

    private void DespawnQuestions()
    {
        animator.Play("UI Animation backwards");
        soundManager.PlayClickSound(5);
    }

    public void HideUI()
    {
        questionImage.SetActive(false);
        escapeButton.SetActive(false);
    }
    
}
