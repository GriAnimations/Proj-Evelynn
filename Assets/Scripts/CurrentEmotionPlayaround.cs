using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrentEmotionPlayaround : MonoBehaviour
{
    [SerializeField] private EmotionManager emotionManager;
    [SerializeField] private CubismModel live2DModel;
    [SerializeField] private BlinkingStuff eyeStuff;
    [SerializeField] private LookingStateManager lookingStateManager;
    
    private float[] _currentValues = new float[33];
    
    private Coroutine _coroutine;

    public void StopPlaying()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
    }
    
    public void StartPlaying()
    {
        for (var i = 0; i < emotionManager.currentActionUnits.Length; i++)
        {
            if (emotionManager.currentActionUnits[i] > 0)
            {
                _coroutine = StartCoroutine(Playing(i));
            }
        }
    }

    private IEnumerator Playing(int currentAction)
    {
        var elapsedTime = 0f;
        var timeDelay = Random.Range(1f, 2f);
        var currentValue = emotionManager.currentActionUnits[currentAction];
        var nextValue = currentValue + Random.Range(-0.2f, 0.4f);
        
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            emotionManager.currentActionUnits[currentAction] = Mathf.Lerp(currentValue, nextValue, preValue);
            
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.InOutBack(normalizedTime);

            emotionManager.currentActionUnits[currentAction] = nextValue + (currentValue - nextValue) * preValue;
            
            yield return null;
        }
    }

    public void EaseEmotions(int currentAction, float decrease)
    {
        StartCoroutine(EasingEmotions(currentAction, decrease));
    }

    private IEnumerator EasingEmotions(int currentAction, float decrease)
    {
        var elapsedTime = 0f;
        var timeDelay = Random.Range(2f, 3f);
        var currentValue = emotionManager.currentActionUnits[currentAction];
        var nextValue = currentValue + decrease;
        
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.InOutBack(normalizedTime);
            
            emotionManager.currentActionUnits[currentAction] = currentValue + (nextValue - currentValue) * preValue;
            
            yield return null;
        }

        if (nextValue <= 0)
        {
            emotionManager.currentActionUnits[currentAction] = 0;
        }
    }

    public void StartPlaySpecificAction(int action, float time, float intensity)
    {
        StartCoroutine(PlaySpecificAction(action, time, intensity));
    }

    private IEnumerator PlaySpecificAction(int action, float time, float intensity)
    {
        var elapsedTime = 0f;
        var timeDelay = time;
        var currentValue = emotionManager.currentActionUnits[action];
        var nextValue = currentValue + intensity;
        
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            emotionManager.currentActionUnits[action] = Mathf.Lerp(currentValue, nextValue, preValue);
            
            yield return null;
        }
        
        timeDelay = Random.Range(1f, 2f);
        nextValue = emotionManager.currentActionUnits[action];
        
        elapsedTime = 0f;
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.InOutBack(normalizedTime);

            emotionManager.currentActionUnits[action] = nextValue + (currentValue - nextValue) * preValue;
            
            yield return null;
        }
    }

    public void StartPLaySpecificMouth(string action, float time, float intensity)
    {
        StartCoroutine(PlaySpecificMouth(action, time, intensity));
    }

    private IEnumerator PlaySpecificMouth(string action, float time, float intensity)
    {
        lookingStateManager.mouthStuffOngoing = true;
        var elapsedTime = 0f;
        var timeDelay = time;
        var index = live2DModel.Parameters.ToList().FindIndex(p => p.Id == action);
        var currentValue = emotionManager.currentActionUnits[index];
        var nextValue = intensity;
        
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            live2DModel.Parameters[index].Value = currentValue + (nextValue - currentValue) * preValue;

            
            yield return null;
        }

        nextValue = live2DModel.Parameters[index].Value;
        
        timeDelay = Random.Range(1f, 2f);
        elapsedTime = 0f;
        while (elapsedTime <= timeDelay)
        {
            if (emotionManager.newEmotion) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / timeDelay);
            var preValue = EasingFunctions.InOutBack(normalizedTime);

            live2DModel.Parameters[index].Value = nextValue + (0 - nextValue) * preValue;
            
            yield return null;
        }

        yield return null;
        lookingStateManager.mouthStuffOngoing = false;
    }
}
