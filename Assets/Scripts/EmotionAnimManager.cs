using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using uLipSync;
using Unity.VisualScripting;
using UnityEngine;

public class EmotionAnimManager : MonoBehaviour
{
    //[SerializeField] private float decayDuration;
    [SerializeField] private float blendDuration;

    //private float _currentValue;

    //private float _initialValue;
    private float _setValue;

    private Coroutine _coroutine;
    
    private CubismParameterStore _store;
    [SerializeField] private float[] actionUnitIntensity;

    public class BlendShapeInfo
    {
        public string phoneme;
        public int index = -1;
        public float maxWeight = 1f;

        public float weight { get; set; } = 0f;
        public float weightVelocity { get; set; } = 0f;
    }

    public List<BlendShapeInfo> blendShapes = new List<BlendShapeInfo>();
    [SerializeField] private CubismModel live2DModel;

    LipSyncInfo _info = new LipSyncInfo();

    private JsonReturn _jsonFile;

    private int[] _actionUnit;
    private char[] _intensityLetter;
    private float[] _intensity;

    [SerializeField] private float holdEmotion;
    [SerializeField] private float[] currentIntensitiesStore;

    private bool _changeEmotionAllowed;
    //[SerializeField] private int[] thisOne;
    private int _emotionCounter;

    private List<int> _filteredList = new();
    private bool _decaying;


    private void Start()
    {
        _changeEmotionAllowed = true;
        
        _jsonFile = new JsonReturn(new[]
        {
            new PhraseFacsPair("Phrase 1", new[] {"12E"}),
            new PhraseFacsPair("Phrase 2", new[] {"1C", "6D"}),
            new PhraseFacsPair("Phrase 3", new[] {"12B", "4C"}),
            new PhraseFacsPair("Phrase 4", new[] {"12E", "4E", "1C"})
        });
        
       ChangeEmotionInput(); 
    }

    private void StartChange()
    {
        if (!_changeEmotionAllowed) return;
        ChangeEmotionInput();
        _changeEmotionAllowed = false;
    }

    private void ChangeEmotionInput()
    {
        if (_emotionCounter < _jsonFile.PhraseFacsPairs.Length)
        {
            SeparateNumberLetterPairs(_jsonFile.PhraseFacsPairs[_emotionCounter].FacsCodes, out _actionUnit, out _intensityLetter);
            _emotionCounter++;
        }
        else
        {
            Debug.Log("done");
        }
        
    }

    private void SeparateNumberLetterPairs(string[] input, out int[] numbers, out char[] letters)
    {
        int length = input.Length;
        numbers = new int[length];
        letters = new char[length];

        for (int i = 0; i < length; i++)
        {
            string item = input[i];
            
            // Extract number part
            string numberPart = "";
            int j = 0;
            while (j < item.Length && char.IsDigit(item[j]))
            {
                numberPart += item[j];
                j++;
            }

            // Convert number part to integer and store in numbers array
            numbers[i] = int.Parse(numberPart);
            
            // Store the letter part in the letters array
            letters[i] = item[j];
        }
        
        IntensityCalculator(letters, out _intensity);
    }

    private void IntensityCalculator(char[] input, out float[] intensity)
    {
        intensity = new float[input.Length];
        int lenght = 0;
        foreach (var x in _intensityLetter)
        {
            intensity[lenght] = x switch
            {
                'A' => 0.2f,
                'B' => 0.4f,
                'C' => 0.6f,
                'D' => 0.8f,
                'E' => 1f,
                _ => intensity[lenght]
            };
            lenght++;
        }
    }

    private void Update()
    { 
        if (!Input.GetKeyDown(KeyCode.A)) return;
        BeginFacialAnimation();
    }

    private void BeginFacialAnimation()
    {
        int counter = 0;
        foreach (var au in _actionUnit)
        {
            var counter1 = counter;
            StartCoroutine(BlendEmotions(au, _intensity[counter], blendDuration, result =>
            {
                currentIntensitiesStore[counter1] = result;
            }));
            
            //thisOne[counter] = au;
            
            counter++;
        }
        
        StartCoroutine(WaitForEmotionDuration());
    }

    private void DecayToZero()
    {
        //List<int> filteredList = new List<int>();
        var counter2 = 0;
        foreach (var x in actionUnitIntensity)
        {
            if (x != 0)
            {
                foreach (var au in _actionUnit)
                {
                    _filteredList.Add(counter2);
                }
            }
            counter2++;
        }
    }

    private void BeginDecay()
    {
        foreach (var au in _filteredList)
        {
            StartCoroutine(BlendEmotions(au, 0, blendDuration, result =>
            {
                currentIntensitiesStore[au] = result;
            }));
        }
        _decaying = true;
    }

    private void LateUpdate()
    {
        ApplyAnimation();
    }

    private void ApplyAnimation()
    {
        int counter = 0;
        foreach (var au in _actionUnit)
        {
            Debug.Log(au);
            live2DModel.Parameters[live2DModel.Parameters.ToList().FindIndex(p => p.Id == "AU"+au)].Value = currentIntensitiesStore[counter];
            counter++;
        }
        

        if (_filteredList.Count > 0 && _decaying)
        {
            foreach (var au in _filteredList)
            { 
                live2DModel.Parameters[live2DModel.Parameters.ToList().FindIndex(p => p.Id == "AU"+au)].Value = currentIntensitiesStore[au];
            }
        }
        
    }

    //private IEnumerator GrowToInitialValue(float holdCurrentEmotion, float nextEmotion)
    //{
    //    var elapsedTime = 0f;
    //    var startValue = _currentValue;
//
    //    while (elapsedTime < growDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        var normalizedTime = Mathf.Clamp01(elapsedTime / growDuration);
    //        var easedTime = EaseInOutCirc(normalizedTime);
//
    //        _currentValue = Mathf.Lerp(startValue, _setValue, easedTime);
//
    //        yield return null;
    //    }
//
    //    _currentValue = _setValue;
//
    //    yield return new WaitForSeconds(holdCurrentEmotion); //affect this when its the last emotion for a random time
//
    //    elapsedTime = 0f;
//
    //    while (elapsedTime < decayDuration) // decay Duration = time to next Emotion
    //    {
    //        elapsedTime += Time.deltaTime;
    //        var normalizedTime = Mathf.Clamp01(elapsedTime / decayDuration);
    //        var easedTime = EaseInOutCirc(normalizedTime);
    //        _currentValue = Mathf.Lerp(_setValue, nextEmotion, easedTime);
    //        yield return null;
    //    }
//
    //    _currentValue = 0;
    //}
    
    
    private IEnumerator BlendEmotions(int actionUnitName, float nextIntensity, float blendDurationInside, System.Action<float> callback)
    {
        var elapsedTime = 0f;
        var startIntensity = actionUnitIntensity[actionUnitName];

        while (elapsedTime < blendDurationInside)
        {
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / blendDurationInside);
            var easedTime = EaseInOutCirc(normalizedTime);

            var value = Mathf.Lerp(startIntensity, nextIntensity, easedTime);
            actionUnitIntensity[actionUnitName] = value;
            callback(value);

            yield return null;
        }

        actionUnitIntensity[actionUnitName] = nextIntensity;
    }

    private IEnumerator WaitForEmotionDuration()
    {
        
        DecayToZero();
        yield return new WaitForSeconds(blendDuration);
        BeginDecay();
        StartChange();
        yield return new WaitForSeconds(blendDuration);
        _decaying = false;
    }


    private float EaseInOutCirc(float x)
    {
        return x < 0.5f
            ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2
            : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
    }
}