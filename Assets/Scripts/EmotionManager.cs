using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class EmotionManager : MonoBehaviour
{
    [SerializeField] private CubismModel live2DModel;
    private float _blendDuration;
    public float[] currentActionUnits;
    [SerializeField] private float[] targetActionUnits;
    
    [SerializeField] private CurrentEmotionPlayaround playaround;
    [SerializeField] private BlinkingStuff eyeStuff;
    public bool newEmotion;

    private int _phrasePairCounter;
    private float _wholeDuration;
    private string _response;
    
    private JsonReturn _jsonFile;

    public class BlendShapeInfo
    {
        public string phoneme;
        public int index = -1;
        public float maxWeight = 1f;

        public float weight { get; set; } = 0f;
        public float weightVelocity { get; set; } = 0f;
    }
    
    private float CalculateBlendTime(float wholeDuration, string response, string currentPhrase)
    {
        var totalCharacters = response.Length;
        var timePerCharacter = wholeDuration / totalCharacters;
        var phraseStartIndex = response.IndexOf(currentPhrase, StringComparison.Ordinal);
        var startTime = phraseStartIndex * timePerCharacter;
        
        return startTime;
    }
    
    private float _currentBlendTime;

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
        //start it all
        
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            targetActionUnits[i] = 0;
        }

        if (_phrasePairCounter < _jsonFile.PhraseFacsPairs.Length)
        {
            _currentBlendTime = CalculateBlendTime(_wholeDuration, _response, _jsonFile.PhraseFacsPairs[_phrasePairCounter].Phrase);
            SeparateNumberLetterPairs(_jsonFile.PhraseFacsPairs[_phrasePairCounter].FacsCodes);
            _phrasePairCounter++;
            
            CheckActionUnitDifference();
        }
        else
        {
            newEmotion = false;
            _currentBlendTime = 0;
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
        _blendDuration = UnityEngine.Random.Range(0.5f, 3f);
        
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            if (Mathf.Approximately(targetActionUnits[i], currentActionUnits[i])) continue;
            _blendDuration += UnityEngine.Random.Range(-0.2f, 0.2f);
            StartCoroutine(BlendEmotions(i, _currentBlendTime));
        }

        StartCoroutine(WaitForNextEmotion());
    }
    
    private IEnumerator BlendEmotions(int actionUnitName, float blendDurationInside)
    {
        var elapsedTime = 0f;
        var startIntensity = currentActionUnits[actionUnitName];
        var targetIntensity = targetActionUnits[actionUnitName];

        if (currentActionUnits[actionUnitName] < targetActionUnits[actionUnitName])
        {
            eyeStuff.EyeColourDecider(actionUnitName);
        }

        var randomizedBlendDuration = blendDurationInside + UnityEngine.Random.Range(0.1f, 0.3f);

        while (elapsedTime < randomizedBlendDuration)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / blendDurationInside);
            var easedTime = OutBack(normalizedTime);
            var value = startIntensity + (targetIntensity - startIntensity) * easedTime;
            currentActionUnits[actionUnitName] = value;
            yield return null;
        }
        
        currentActionUnits[actionUnitName] = targetIntensity;
    }
    

    private static float OutBack(float t) => 1 - EasingFunctions.InBack(1 - t);
    

    private IEnumerator WaitForNextEmotion()
    {
        yield return new WaitForSeconds(_blendDuration + 0.3f);
        
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
