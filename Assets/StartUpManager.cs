using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUpManager : MonoBehaviour
{
    private static readonly int Status = Animator.StringToHash("status");

    [SerializeField] private Image blackBackGround;
    [SerializeField] private GameObject contract;
    [SerializeField] private GameObject wholeCanvas;
    
    [SerializeField] private float fadeDuration;
    [SerializeField] private GameObject logo;
    
    [SerializeField] private Sprite[] buttonSprites;
    [SerializeField] private Button startButton;
    
    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private Animator animator;
    
    [SerializeField] private ResponseManager responseManager;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeOutBlackBackGround());
    }

    private IEnumerator FadeOutBlackBackGround()
    {
        yield return new WaitForSeconds(fadeDuration);
        
        var elapsedTime = 0f;
        while (elapsedTime <= 2f)
        {
            elapsedTime += Time.deltaTime;
            
            var normalizedTime = Mathf.Clamp01(elapsedTime / 2f);
            var alpha = Mathf.Lerp(1, 0.85f, normalizedTime);
            blackBackGround.color = new Color(0, 0, 0, alpha);

            if (elapsedTime >= 1.5f) SpawnContract();

            yield return null;
        }
        
        logo.gameObject.SetActive(false);
    }

    private void SpawnContract()
    {
        animator.Play("Contract Spawn");
    }

    public void DespawnContract()
    {
        animator.Play("Contract Despawn");
    }

    public void DeactivateItForReal()
    {
        questionManager.ToggleQuestions();
        wholeCanvas.SetActive(false);
        responseManager.allowedToSpeak = true;
    }

    public void ActivateItForReal()
    {
        contract.SetActive(true);
    }
}
