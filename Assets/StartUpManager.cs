using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUpManager : MonoBehaviour
{

    [SerializeField] private Image blackBackGround;
    [SerializeField] private GameObject contract;
    [SerializeField] private GameObject wholeCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeOutBlackBackGround());
    }

    private IEnumerator FadeOutBlackBackGround()
    {
        var elapsedTime = 0f;
        while (elapsedTime <= 2f)
        {
            elapsedTime += Time.deltaTime;
            
            var normalizedTime = Mathf.Clamp01(elapsedTime / 2f);
            var alpha = Mathf.Lerp(1, 0, normalizedTime);
            blackBackGround.color = new Color(0, 0, 0, alpha);

            if (elapsedTime >= 1.5f) SpawnContract();

            yield return null;
        }
        
        
        blackBackGround.color = new Color(0, 0, 0, 0);
    }

    private void SpawnContract()
    {
        contract.SetActive(true);
    }

    public void DespawnContract()
    {
        contract.SetActive(false);
        wholeCanvas.SetActive(false);
        //replace this with anim and do anim event with the other one
        
        //also start the bootup anim here
    }

    public void DeactivateItForReal()
    {
        contract.SetActive(false);
    }
}
