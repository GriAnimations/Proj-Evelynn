using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class EmotionManager : MonoBehaviour
{
    [SerializeField] private CubismModel live2DModel;
    public float[] currentActionUnits;
    [SerializeField] private float[] targetActionUnits;
    
    [SerializeField] private CurrentEmotionPlayaround playaround;
    [SerializeField] private BlinkingStuff eyeStuff;
    public bool newEmotion;
    public bool talkingLookChange;

    private int _phrasePairCounter;
    private float _wholeDuration;
    private string _response;

    private bool _isEasing;
    
    private JsonReturn _jsonFile;
    
    private float CalculateBlendTime(float wholeDuration, string response, string currentPhrase)
    {
        var totalCharacters = response.Length;
        var timePerCharacter = wholeDuration / totalCharacters;
        //var phraseStartIndex = response.IndexOf(currentPhrase, StringComparison.Ordinal);
        
        var pattern = $@"\b{Regex.Escape(currentPhrase)}\b";
        var match = Regex.Match(response, pattern, RegexOptions.IgnoreCase);
        var phraseIndex = match.Index;
        
        var startTime = phraseIndex * timePerCharacter;
        
        return startTime;
    }
    
    private float _currentBlendTime;
    private float _allBlendTimes;
    private float _randomizedEmotionBlend;

    public void StartNewEmotion(JsonReturn emotionJson, float wholeDuration, string response)
    {
        _jsonFile = emotionJson;
        _wholeDuration = wholeDuration;
        _response = response;
        _phrasePairCounter = 0;
        
        NewEmotionInput();
    }
    

    private void NewEmotionInput()
    {
        playaround.StopPlaying();
        newEmotion = true;
        
        Debug.Log("new emotions triggered");
        //start it all
        
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            targetActionUnits[i] = 0;
        }

        if (_phrasePairCounter < _jsonFile.PhraseFacsPairs.Length)
        {
            _currentBlendTime = CalculateBlendTime(_wholeDuration, _response, _jsonFile.PhraseFacsPairs[_phrasePairCounter].Phrase) - _allBlendTimes;
            _allBlendTimes += _currentBlendTime;
            
            SeparateNumberLetterPairs(_jsonFile.PhraseFacsPairs[_phrasePairCounter].FacsCodes);
            _phrasePairCounter++;
            
            CheckActionUnitDifference();
            
            talkingLookChange = true;
        }
        else
        {
            newEmotion = false;
            _currentBlendTime = 0;
            _allBlendTimes = 0;
            _isEasing = false;
            StartCoroutine(PlayOccacionally());
        }
    }
    
    private void SeparateNumberLetterPairs(string[] input)
    {
        foreach (var au in input)
        {
            var letter = au.Substring(au.Length - 1);

            var numberPart = "";
            
            var j = 0;
            while (j < au.Length && char.IsDigit(au[j]))
            {
                numberPart += au[j];
                j++;
            }

            IntensityCalculator(letter, out var intensity);

            if (int.TryParse(numberPart, out var number) && number is >= 0 and <= 31)
            {
                targetActionUnits[number] = intensity;
            }
            else
            {
                Debug.Log("who tf this emotion");
            }
        }
        
        //CheckActionUnitDifference();
    }

    private void IntensityCalculator(string input, out float intensity)
    {
    intensity = input switch
        {
            "A" => UnityEngine.Random.Range(0.1f, 0.25f),
            "B" => UnityEngine.Random.Range(0.3f, 0.45f),
            "C" => UnityEngine.Random.Range(0.5f, 0.65f),
            "D" => UnityEngine.Random.Range(0.7f, 0.85f),
            "E" => UnityEngine.Random.Range(0.9f, 1f),
            _ => 0f
        };
    }


    private void CheckActionUnitDifference()
    {
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            if (Mathf.Approximately(targetActionUnits[i], currentActionUnits[i])) continue;
            StartCoroutine(BlendEmotions(i));
        }

        StartCoroutine(WaitForNextEmotion());
    }
    
    private IEnumerator BlendEmotions(int actionUnitName)
    {
        var elapsedTime = 0f;
        var startIntensity = currentActionUnits[actionUnitName];
        var targetIntensity = targetActionUnits[actionUnitName];

        if (currentActionUnits[actionUnitName] < targetActionUnits[actionUnitName])
        {
            eyeStuff.EyeColourDecider(actionUnitName);
        }

        _randomizedEmotionBlend = UnityEngine.Random.Range(0.8f, 1.5f);

        while (elapsedTime <= _randomizedEmotionBlend)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / _randomizedEmotionBlend);
            var easedTime = EasingFunctions.OutBack(normalizedTime);
            var value = startIntensity + (targetIntensity - startIntensity) * easedTime;
            currentActionUnits[actionUnitName] = value;
            yield return null;
        }
        
        currentActionUnits[actionUnitName] = targetIntensity;

        while (elapsedTime <= 20f && _isEasing)
        {
            if (!_isEasing) break;
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / 20f);
            var easedTime = EasingFunctions.InOutSine(normalizedTime);
            var value = Mathf.Lerp(targetIntensity, 0, easedTime);
            currentActionUnits[actionUnitName] = value;
            yield return null;
        }
    }
    
    private IEnumerator WaitForNextEmotion()
    {
        _isEasing = true;
        if (_randomizedEmotionBlend > _currentBlendTime)
        {
            yield return new WaitForSeconds(_randomizedEmotionBlend);
        }
        else
        {
            yield return new WaitForSeconds(_currentBlendTime);
        }
        _isEasing = false;
        
        NewEmotionInput();
    }

    private IEnumerator PlayOccacionally()
    {
        while (!newEmotion)
        {
            var rando = UnityEngine.Random.Range(5f, 10f);
            if (newEmotion)
            {
                break;
            }
            yield return new WaitForSeconds(rando);
            playaround.StartPlaying();
        }
    }
   
    
    private void LateUpdate()
    {
        ApplyAnimation();
    }

    private void ApplyAnimation()
    {
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            var index = live2DModel.Parameters.ToList().FindIndex(p => p.Id == "AU" + i);
            if (index == -1) continue;
            live2DModel.Parameters[index].Value = currentActionUnits[i];
        }
    }
}
