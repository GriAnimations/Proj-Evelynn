using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class LookingStateManager : MonoBehaviour
{
    LookingBaseState currentState;
    public Bored BoredState = new Bored();
    public Attention AttentionState = new Attention();
    public Talking TalkingState = new Talking();
    public Listening ListeningState = new Listening();
    public Thinking ThinkingState = new Thinking();
    
    public CubismModel live2DModel;
    [SerializeField] private CurrentEmotionPlayaround playaround;
    public EmotionManager emotionManager;
    [SerializeField] private BlinkingStuff blinkingStuff;
    [SerializeField] private BodyLanguage bodyLanguage;

    public float dartingSpeedLowerEnd = 0.6f;
    public float dartingSpeedUpperEnd = 2f;

    private bool _blinking = true;
    public float lookingSpeed;
    public bool stationaryEyes;

    private float _currentPointX;
    private float _currentPointY;

    private float _currentSubX;
    private float _currentSubY;

    private float _lookPreReCalcX;
    private float _lookPreReCalcY;
    
    private Coroutine _lookingCoroutine;

    private float _turnSpeed;
    public bool waitingDone;
    
    private Coroutine _headTurnCoroutine;

    public bool thinking;
    
    float TargetHeadX(float input)
    {
        return input switch
        {
            >= -1 and <= 1 => 0,
            < -1 and >= -2 => input + 1,
            > 1 and <= 2 => input - 1,
            _ => throw new ArgumentOutOfRangeException(nameof(input))
        };
    }

    
    // Start is called before the first frame update
    void Start()
    {
        dartingSpeedLowerEnd = 0.5f;
        dartingSpeedUpperEnd = 1.3f;
        
        currentState = AttentionState;
        currentState.EnterState(this);

        stationaryEyes = true;
        StartCoroutine(Blinking());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        currentState.UpdateState(this);
        EyeHeadReCalculator();
    }

    public void DoAction(LookingBaseState state)
    {
        currentState = state;
        state.DoAction(this);
    }

    public void SwitchState(LookingBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }


    private IEnumerator Blinking()
    {
        var blinkCd = Random.Range(2f, 10f);
        yield return new WaitForSeconds(blinkCd);
        
        var elapsedTime = 0f;
        while (elapsedTime <= 0.04f)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / 0.04f);
            
            var value = Mathf.Lerp(0, 1, normalizedTime);
            
            live2DModel.Parameters[0].Value = value;
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime <= 0.2f)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / 0.2f);
            
            var preValue = Mathf.Lerp(1, 0, normalizedTime);
            var value = InQuad(preValue);
            
            live2DModel.Parameters[0].Value = value;
            yield return null;
        }
        live2DModel.Parameters[0].Value = 0;

        if (_blinking)
        {
            StartCoroutine(Blinking());
        }
    }
    
    private static float InQuad(float t) => t * t;

    public void ChoosePoint(float x, float y)
    {
        stationaryEyes = false;
        if (_lookingCoroutine != null)
        {
            StopCoroutine(_lookingCoroutine);
        }
        _lookingCoroutine = StartCoroutine(MoveToPoint(x, y));
    }

    private IEnumerator MoveToPoint(float x, float y)
    {
        var elapsedTime = 0f;
        _currentSubX = _lookPreReCalcX;
        _currentSubY = _lookPreReCalcY;

        _currentPointX = x;
        _currentPointY = y;

        while (elapsedTime <= lookingSpeed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / lookingSpeed);
            
            var preValue = EasingFunctions.InOutCubic(normalizedTime);

            var valueX = Mathf.Lerp(_currentSubX, x, preValue);
            var valueY = Mathf.Lerp(_currentSubY, y, preValue);
            
            _lookPreReCalcX = valueX;
            _lookPreReCalcY = valueY;
            
            yield return null;
        }

        if (_headTurnCoroutine != null)
        {
            StopCoroutine(_headTurnCoroutine);
        }
        _headTurnCoroutine = StartCoroutine(HeadTurn());
        
        stationaryEyes = true;

        _currentSubX = _lookPreReCalcX;
        _currentSubY = _lookPreReCalcY;
        
        var chillShortly = 0;
        
        while (stationaryEyes)
        {
            var dartingSpeed = Random.Range(dartingSpeedLowerEnd, dartingSpeedUpperEnd);
            if (dartingSpeed >= 1f && chillShortly >= 2)
            {
                ChooseSubPoint(x, y);
            }
        
            yield return new WaitForSeconds(dartingSpeed);
            _lookPreReCalcX = _currentSubX + Random.Range(-0.07f, 0.07f);
            _lookPreReCalcY = _currentSubY + Random.Range(-0.07f, 0.07f);
            
            chillShortly++;
            yield return null;
        }
    }

    private void ChooseSubPoint(float x, float y)
    {
        var subX = x + Random.Range(-0.1f, 0.1f);
        var subY = y + Random.Range(-0.2f, 0.2f);
        
        StartCoroutine(MoveToSubPoint(subX, subY));
    }

    private IEnumerator MoveToSubPoint(float subX, float subY)
    {
        //yield return new WaitForSeconds(2f);
        
        var elapsedTime = 0f;

        var preSubX = _currentSubX;
        var preSubY = _currentSubY;

        while (elapsedTime <= 0.2f)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / 0.2f);
            
            var preValue = EasingFunctions.InOutCubic(normalizedTime);
            
            _currentSubX = Mathf.Lerp(preSubX, subX, preValue);
            _currentSubY = Mathf.Lerp(preSubY, subY, preValue);
            
            yield return null;
        }
    }
    
    private void EyeHeadReCalculator()
    {
        live2DModel.Parameters[1].Value = _lookPreReCalcX - live2DModel.Parameters[27].Value;
        live2DModel.Parameters[2].Value = _lookPreReCalcY - live2DModel.Parameters[28].Value;
    }

    private IEnumerator HeadTurn()
    {
        var elapsedTime = 0f;
        
        var currentHeadRotationX = live2DModel.Parameters[27].Value;
        //var currentHeadRotationY = live2DModel.Parameters[28].Value;
        
        var currentLookTargetX = _currentPointX;
        //var currentLookTargetY = _currentPointY;
        _turnSpeed = Random.Range(0.8f, 2.5f);
        
        var randomChoose = Random.Range(0, 4);
        
        while (elapsedTime <= _turnSpeed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / _turnSpeed);
            
            var preValue = randomChoose switch
            {
                0 => EasingFunctions.OutBack(normalizedTime),
                1 => EasingFunctions.OutQuad(normalizedTime),
                2 => EasingFunctions.OutCirc(normalizedTime),
                _ => EasingFunctions.OutQuint(normalizedTime)
            };

            //var preValue = EasingFunctions.OutQuint(normalizedTime);
            var valueX = currentHeadRotationX + (TargetHeadX(currentLookTargetX) - currentHeadRotationX) * preValue;
            //var valueX = Mathf.Lerp(currentHeadRotationX, targetHeadX(currentLookTargetX), preValue);
            //var valueY = Mathf.Lerp(currentHeadRotationY, targetHeadX(currentLookTargetY), preValue);

            live2DModel.Parameters[27].Value = valueX;
            //live2DModel.Parameters[28].Value = valueY;

            yield return null;
        }
    }
    
    public void StartDistracted()
    {
        StartCoroutine(Distracted());
    }

    private IEnumerator Distracted()
    {
        SwitchState(BoredState);
        yield return new WaitForSeconds(Random.Range(2.5f, 5f));
        if (currentState == BoredState)
        {
            SwitchState(AttentionState);
        }
    }

    public void EaseEmotions()
    {
        for (var i = 0; i < emotionManager.currentActionUnits.Length; i++)
        {
            if (emotionManager.currentActionUnits[i] > 0)
            {
                playaround.EaseEmotions(i);
            }
        }
    }

    public void Wait(float time)
    {
        StartCoroutine(Waiting(time));
    }

    private IEnumerator Waiting(float time)
    {
        yield return new WaitForSeconds(time);
        waitingDone = true;
    }

    public void StartBlinkingLights()
    {
        thinking = true;
        blinkingStuff.StartInternetBlink();
        blinkingStuff.StartEyesBlink();
    }

    public void StartSpecificEmotion(int action, float time, float intenstiy)
    {
        playaround.StartPlaySpecificAction(action, time, intenstiy);
    }

    public void StartSpecificMouth(string action, float time, float intensity)
    {
        playaround.StartPLaySpecificMouth(action, time, intensity);
    }
}
