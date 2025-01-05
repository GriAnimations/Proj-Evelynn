using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Core;
using UnityEngine;

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
            new PhraseFacsPair("Phrase 1", new[] {"12E", "6A", "1C"}),
            new PhraseFacsPair("Phrase 2", new[] {"1C", "6D"}),
            new PhraseFacsPair("Phrase 3", new[] {"12B", "4C"}),
            new PhraseFacsPair("Phrase 4", new[] {"12E", "4E", "1C"})
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
            //Debug.Log("Emotion Number: " + _phrasePairCounter);
        }
        else
        {
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
        
        CheckActionUnitDifference();
    }

    private void IntensityCalculator(string input, out float intensity)
    {
    intensity = input switch
        {
            "A" => 0.2f,
            "B" => 0.4f,
            "C" => 0.6f,
            "D" => 0.8f,
            "E" => 1f,
            _ => 0f
        };
    }


    private void CheckActionUnitDifference()
    {
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
            var easedTime = EaseInOutCirc(normalizedTime);

            var value = Mathf.Lerp(startIntensity, targetIntensity, easedTime);
            currentActionUnits[actionUnitName] = value;

            yield return null;
        }
        

        currentActionUnits[actionUnitName] = targetIntensity;
    }
    
    private float EaseInOutCirc(float x)
    {
        return x < 0.5f
            ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2
            : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
    }

    private IEnumerator WaitForNextEmotion()
    {
        yield return new WaitForSeconds(blendDuration + 1f);
        
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
            //if (!Mathf.Approximately(targetActionUnits[i], currentActionUnits[i]))
            //{
            //    live2DModel.Parameters[index].Value = currentActionUnits[12];
            //    //live2DModel.Parameters[index].Value = currentActionUnits[i];
            //}
        }
    }
}
