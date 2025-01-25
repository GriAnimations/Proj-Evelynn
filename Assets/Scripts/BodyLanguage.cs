using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;

public class BodyLanguage : MonoBehaviour
{
    public CubismModel live2DModel;
    [SerializeField] private EmotionManager emotionManager;
    [SerializeField] private LookingStateManager lookingStateManager;
    
    private Coroutine _coroutine45;
    private Coroutine _coroutine46;
    private Coroutine _coroutine29;

    public bool breathing;

    public float shockIncrease;
    public float breatheIncrease;

    private void Start()
    {
        breathing = true;
        StartCoroutine(Breathing());
    }

    private IEnumerator Breathing()
    {
        while (breathing)
        {
            var breathOut = UnityEngine.Random.Range(-6f, -4f);
            var breathIn = UnityEngine.Random.Range(8f, 4f);
            var randomTime = UnityEngine.Random.Range(2f, 4f) + breatheIncrease;
            
            var elapsedTime = 0f;
            while (elapsedTime <= randomTime)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                var easedTime = EasingFunctions.InOutCubic(normalizedTime);
                var value = Mathf.Lerp(breathOut, breathIn, easedTime);
                lookingStateManager.headYIncrease = Mathf.Lerp(-0.2f, 0.2f, easedTime);
                live2DModel.Parameters[44].Value = value + shockIncrease;
                
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            
            elapsedTime = 0f;
            while (elapsedTime <= randomTime)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                var easedTime = EasingFunctions.InOutCubic(normalizedTime);
                var value = Mathf.Lerp(breathIn, breathOut, easedTime);
                lookingStateManager.headYIncrease = Mathf.Lerp(0.2f, -0.2f, easedTime);
                live2DModel.Parameters[44].Value = value + shockIncrease;
                
                yield return null;
            }
        }
    }

    public void BodyPosition(int index, float target, float speed)
    {
        switch (index)
        {
            case 45:
                if (_coroutine45 != null)
                {
                    StopCoroutine(_coroutine45);
                }
                _coroutine45 = StartCoroutine(MoveToBodyPosition(index, target * 30, speed));
                break;
            case 46:
                if (_coroutine46 != null)
                {
                    StopCoroutine(_coroutine46);
                }
                _coroutine46 = StartCoroutine(MoveToBodyPosition(index, target * 30, speed));
                break;
            case 29:
                if (_coroutine29 != null)
                {
                    StopCoroutine(_coroutine29);
                }
                _coroutine29 = StartCoroutine(MoveToBodyPosition(index, target, speed));
                break;
        }
    }

    private IEnumerator MoveToBodyPosition(int index, float target, float speed)
    {
        var elapsedTime = 0f;
        var currentValue = live2DModel.Parameters[index].Value;
        
        while (elapsedTime <= speed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / speed);
            var easedTime = EasingFunctions.InOutBack(normalizedTime);
            var value = currentValue + (target - currentValue) * easedTime;
            
            live2DModel.Parameters[index].Value = value;
                
            yield return null;
        }
    }
}
