using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class LookingStateManager : MonoBehaviour
{
    public LookingBaseState currentState;
    public Bored BoredState = new Bored();
    public Attention AttentionState = new Attention();
    public Talking TalkingState = new Talking();
    public Listening ListeningState = new Listening();
    public Thinking ThinkingState = new Thinking();
    public Asleep AsleepState = new Asleep();
    
    public CubismModel live2DModel;
    [SerializeField] private CurrentEmotionPlayaround playaround;
    public EmotionManager emotionManager;
    [SerializeField] private BlinkingStuff blinkingStuff;
    [SerializeField] private BodyLanguage bodyLanguage;
    public SoundManager soundManager;
    public Button resetButton;

    public bool mouthStuffOngoing;

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
    public float headYIncrease;

    private float _blinkingReduce;

    public bool automaticHead;
    public bool wasJustBored;

    private bool _veryFirstTime;

    float FinalHeadY(float input)
    {
        return input + headYIncrease;
    }
    
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

        bodyLanguage.breatheIncrease = 2f;
        
        currentState = AsleepState;
        currentState.EnterState(this);

        stationaryEyes = true;
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

    public void StopSleeping()
    {
        blinkingStuff.OriginalColour();
        AsleepState.StillAsleep = false;
        bodyLanguage.breatheIncrease = 0;
        _blinking = true;
        _blinkingReduce = 10f;
        StartCoroutine(Blinking());
    }

    private IEnumerator Blinking()
    {
        while (_blinking)
        {
            var blinkCd = Random.Range(2f, 10f);
        
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

            if (_blinkingReduce > 0)
            {
                _blinkingReduce -= 1;
            }
            
            yield return new WaitForSeconds(blinkCd / _blinkingReduce);
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

        
        if (_headTurnCoroutine != null || !automaticHead && _headTurnCoroutine != null)
        {
            StopCoroutine(_headTurnCoroutine);
        }
        
        if (automaticHead)
        {
            _headTurnCoroutine = StartCoroutine(HeadTurn());
        }
        
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
        var currentHeadRotationY = live2DModel.Parameters[28].Value;
        
        var currentLookTargetX = _currentPointX;
        var currentLookTargetY = _currentPointY;
        _turnSpeed = Random.Range(0.6f, 1.2f);
        
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
            var valueY = currentHeadRotationY + (TargetHeadX(currentLookTargetY) - currentHeadRotationY) * preValue;
            //var valueX = Mathf.Lerp(currentHeadRotationX, targetHeadX(currentLookTargetX), preValue);
            //var valueY = Mathf.Lerp(currentHeadRotationY, targetHeadX(currentLookTargetY), preValue);

            live2DModel.Parameters[27].Value = valueX;
            live2DModel.Parameters[28].Value = valueY;
            //live2DModel.Parameters[28].Value = FinalHeadY(valueY);

            yield return null;
        }
    }
    
    public void StartDistracted()
    {
        StartCoroutine(Distracted());
    }

    private IEnumerator Distracted()
    {
        wasJustBored = true;
        SwitchState(BoredState);
        yield return new WaitForSeconds(Random.Range(2.5f, 5f));
        if (currentState == BoredState)
        {
            SwitchState(AttentionState);
        }
    }

    public void EaseEmotions(float decrease)
    {
        for (var i = 0; i < emotionManager.currentActionUnits.Length; i++)
        {
            if (emotionManager.currentActionUnits[i] > 0)
            {
                playaround.EaseEmotions(i, decrease);
            }
        }
    }

    public void StartBootUpSequence()
    {
        StartCoroutine(BootUpSequence());
    }

    private IEnumerator BootUpSequence()
    {
        automaticHead = false;
        var currentShock = bodyLanguage.shockIncrease;
        var currentHeadY = live2DModel.Parameters[28].Value;
        
        var elapsedTime = 0f;
        var randomTime = Random.Range(0.3f, 0.4f);
        
        StartSpecificEmotion(4, randomTime, randomTime);
        blinkingStuff.OriginalColour();
        
        ColourChangeWithBlink(new Color(1f, 1f, 1f, 1), 1f, false);
        
        while (elapsedTime <= randomTime)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, -25f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, -0.8f, preValue);
            live2DModel.Parameters[28].Value = FinalHeadY(headY);
            
            yield return null;
        }
        
        elapsedTime = 0f;
        randomTime = Random.Range(0.5f, 0.8f);
        
        currentShock = bodyLanguage.shockIncrease;
        currentHeadY = live2DModel.Parameters[28].Value;
        var currentHeadZ = live2DModel.Parameters[29].Value;
        
        while (elapsedTime <= randomTime)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, 20f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, 0.8f, preValue);
            live2DModel.Parameters[28].Value = FinalHeadY(headY);
            
            live2DModel.Parameters[29].Value = Mathf.Lerp(currentHeadZ, 0f, preValue);
            
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        
        AsleepState.FranticLookAround = true;
        
        elapsedTime = 0f;
        randomTime = Random.Range(1f, 2f);
        
        currentShock = bodyLanguage.shockIncrease;
        currentHeadY = live2DModel.Parameters[28].Value;
        
        while (elapsedTime <= randomTime)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.InOutQuad(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, 0f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, 0f, preValue);
            live2DModel.Parameters[28].Value = headY;
            
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        automaticHead = true;
        resetButton.interactable = true;
    }

    public void InitiateShutDown()
    {
        _blinking = false;
        bodyLanguage.breatheIncrease = 2f;
        
        StartCoroutine(ShutDown());
    }

    private IEnumerator ShutDown()
    {
        var responseManager = FindObjectOfType<ResponseManager>();
        responseManager.allowedToSpeak = false;
        automaticHead = false;
        var currentShock = bodyLanguage.shockIncrease;
        var currentHeadY = live2DModel.Parameters[28].Value;
        
        var elapsedTime = 0f;
        var randomTime = Random.Range(0.3f, 0.4f);
        
        ColourChangeWithBlink(new Color(1f, 0.2f, 0.4f), 1f, false);
        
        EaseEmotions(-1f);
        
        while (elapsedTime <= randomTime)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, -25f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, -0.8f, preValue);
            live2DModel.Parameters[28].Value = FinalHeadY(headY);
            
            yield return null;
        }
        
        elapsedTime = 0f;
        randomTime = Random.Range(1f, 1.1f);
        
        blinkingStuff.StartEyeFade(2.2f);
        
        currentShock = bodyLanguage.shockIncrease;
        currentHeadY = live2DModel.Parameters[28].Value;
        var currentHeadX = live2DModel.Parameters[27].Value;
        
        while (elapsedTime <= randomTime)
        {
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.OutCirc(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, 20f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, 0.8f, preValue);
            live2DModel.Parameters[28].Value = FinalHeadY(headY);
            
            var headX = Mathf.Lerp(currentHeadX, 0f, preValue);
            live2DModel.Parameters[27].Value = headX;
            
            yield return null;
        }
        

        yield return new WaitForSeconds(1f);
        
        elapsedTime = 0f;
        randomTime = Random.Range(2f, 2.5f);
        
        currentShock = bodyLanguage.shockIncrease;
        currentHeadY = live2DModel.Parameters[28].Value;
        
        while (elapsedTime <= randomTime)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
            var preValue = EasingFunctions.InOutCirc(normalizedTime);
            
            bodyLanguage.shockIncrease = Mathf.Lerp(currentShock, -20f, preValue);
            
            var headY = Mathf.Lerp(currentHeadY, -1f, preValue);
            live2DModel.Parameters[28].Value = headY;
            
            live2DModel.Parameters[0].Value = Mathf.Lerp(0f, 2f, preValue);
            
            yield return null;
        }
        
        EaseEmotions(-1f);
        yield return new WaitForSeconds(2f);
        automaticHead = true;
        
        if (!_veryFirstTime)
        {
            _veryFirstTime = true;
        }
        else
        {
            responseManager.allowedToSpeak = true;
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

    public void ColourChangeWithBlink(Color colour, float duration, bool fade)
    {
        blinkingStuff.ColourChange(colour, duration, fade);
    }

    public void StartSpecificEmotion(int action, float time, float intensity)
    {
        playaround.StartPlaySpecificAction(action, time, intensity);
    }

    public void StartSpecificMouth(string action, float time, float intensity)
    {
        if (!mouthStuffOngoing) playaround.StartPLaySpecificMouth(action, time, intensity);
    }

    public void StartSpecificBody(int index, float target, float speed)
    {
        bodyLanguage.BodyPosition(index, target, speed);
    }

    public void ChangeShockIncrease(float value)
    {
        bodyLanguage.shockIncrease += value;
    }

    public void StartNod(float duration, int amount)
    {
        StartCoroutine(Nodding(duration, amount));
    }

    private IEnumerator Nodding(float duration, int amount)
    {
        var overallTime = 0;
        automaticHead = false;
        var previousHeadY = live2DModel.Parameters[28].Value;
        var randomValue = Random.Range(0.3f, 0.7f);
        var randomChance = Random.Range(0, 2);
        
        while (overallTime < amount)
        {
            overallTime++;
            
            var currentHeadY = live2DModel.Parameters[28].Value;
        
            var elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                var value = EasingFunctions.InOutSine(normalizedTime);

                if (randomChance == 0)
                {
                    live2DModel.Parameters[28].Value = currentHeadY + (currentHeadY - randomValue - currentHeadY) * value;
                }
                else
                {
                    live2DModel.Parameters[28].Value = currentHeadY + (currentHeadY + randomValue - currentHeadY) * value;
                }
                
            
                yield return null;
            }
            elapsedTime = 0f;
            currentHeadY = live2DModel.Parameters[28].Value;
            
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                var value = EasingFunctions.InOutSine(normalizedTime);

                if (overallTime == amount - 1)
                {
                    live2DModel.Parameters[28].Value = currentHeadY + (previousHeadY - currentHeadY) * value;
                }
                else
                {
                    if (randomChance == 0)
                    {
                        live2DModel.Parameters[28].Value = currentHeadY + (currentHeadY + randomValue - currentHeadY) * value;
                    }
                    else
                    {
                        live2DModel.Parameters[28].Value = currentHeadY + (currentHeadY - randomValue - currentHeadY) * value;
                    }
                }
                
                yield return null;
            }

            duration *= 0.8f;
            randomValue -= 0.15f;
        }
        
        yield return null;
        automaticHead = true;
    }
}
