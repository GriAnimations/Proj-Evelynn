using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Core;
using UnityEngine;
using Random = System.Random;

public class EmotionManager : MonoBehaviour
{
    [SerializeField] private CubismModel live2DModel;
    [SerializeField] private float blendDuration;
    [SerializeField] private float[] currentActionUnits;
    [SerializeField] private float[] targetActionUnits;
    
    private Coroutine[] _coroutines;

    private int _phrasePairCounter;
    
    //private List<int> _currentActionUnits = new();
    //private List<int> _targetActionUnits = new();
    
    private JsonReturn _jsonFile;

    public class BlendShapeInfo
    {
        public string phoneme;
        public int index = -1;
        public float maxWeight = 1f;

        public float weight { get; set; } = 0f;
        public float weightVelocity { get; set; } = 0f;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        _coroutines = new Coroutine[30];
        
        _jsonFile = new JsonReturn(new[]
        {
            new PhraseFacsPair("Phrase 1", new[] {"12E"}),
            new PhraseFacsPair("Phrase 2", new[] {"12E", "6A", "1C"}),
            new PhraseFacsPair("Phrase 3", new[] {"1C", "6D"}),
            new PhraseFacsPair("Phrase 4", new[] {"12B", "4C"}),
            new PhraseFacsPair("Phrase 5", new[] {"12E", "4E", "1C"}),
            new PhraseFacsPair("Phrase 6", new[] {"9E", "10D", "17C"}),
            new PhraseFacsPair("Phrase 7", new[] {"9B", "10B"}),
            new PhraseFacsPair("Phrase 8", new[] {"15D", "1C"}),
            new PhraseFacsPair("Phrase 9", new[] {"15B"})
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.A)) return;
        _phrasePairCounter = 0;
        NewEmotionInput();
    }

    private void NewEmotionInput()
    {
        //start it all
        
        for (var i = 0; i < targetActionUnits.Length; i++)
        {
            targetActionUnits[i] = 0;
        }

        if (_phrasePairCounter < _jsonFile.PhraseFacsPairs.Length)
        {
            SeparateNumberLetterPairs(_jsonFile.PhraseFacsPairs[_phrasePairCounter].FacsCodes);
            _phrasePairCounter++;
            
            CheckActionUnitDifference();
        }

    }
    
    private void SeparateNumberLetterPairs(string[] input)
    {
        foreach (var au in input)
        {
            var letter = au.Substring(au.Length - 1);

            string numberPart = "";
            int j = 0;
            while (j < au.Length && char.IsDigit(au[j]))
            {
                numberPart += au[j];
                j++;
            }

            IntensityCalculator(letter, out var intensity);

            targetActionUnits[int.Parse(numberPart)] = intensity;
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
        blendDuration = UnityEngine.Random.Range(0.7f, 2f);
        
        for (int i = 0; i < targetActionUnits.Length; i++)
        {
            if (!Mathf.Approximately(targetActionUnits[i], currentActionUnits[i]))
            {
                //_coroutines[i] = StartCoroutine(BlendEmotions(i, blendDuration));
                StartCoroutine(BlendEmotions(i, blendDuration));
            }
        }

        StartCoroutine(WaitForNextEmotion());
    }
    
    private IEnumerator BlendEmotions(int actionUnitName, float blendDurationInside)
    {
        var elapsedTime = 0f;
        var startIntensity = currentActionUnits[actionUnitName];
        var targetIntensity = targetActionUnits[actionUnitName];
        
        //Debug.Log(actionUnitName + "  current: " + currentActionUnits[actionUnitName]);
        //Debug.Log(actionUnitName + "  target: " + targetActionUnits[actionUnitName]);

        while (elapsedTime < blendDurationInside)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / blendDurationInside);
            //var easedTime = EaseInOutCirc(normalizedTime);
            //var easedTime = EaseOutBack(normalizedTime);
            var easedTime = OutBack(normalizedTime);

            //var value = Mathf.Lerp(startIntensity, targetIntensity, easedTime);
            
            var value = startIntensity + (targetIntensity - startIntensity) * easedTime;
            
            currentActionUnits[actionUnitName] = value;

            yield return null;
        }
        

        currentActionUnits[actionUnitName] = targetIntensity;
    }

    private static float InBack(float t)
    {
        float s = 1.5f;
        return t * t * ((s + 1) * t - s);
    }

    private static float OutBack(float t) => 1 - InBack(1 - t);
    

    private IEnumerator WaitForNextEmotion()
    {
        yield return new WaitForSeconds(blendDuration + 0.3f);
        
        NewEmotionInput();
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
