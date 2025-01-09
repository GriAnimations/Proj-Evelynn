using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class LookingStateManager : MonoBehaviour
{
    LookingBaseState currentState;
    public Bored _boredState = new Bored();
    public Attention _attentionState = new Attention();
    public Distracted _distractedState = new Distracted();
    
    public CubismModel live2DModel;
    
    public float currentX;
    public float currentY;

    private bool _blinking = true;
    public float lookingSpeed;
    public bool stationaryEyes;
    
    private Coroutine _lookingCoroutine;

    [SerializeField] private float turnSpeed;
    public bool waitingDone = true;
    
    float targetHeadX(float input)
    {
        return input switch
        {
            >= -1 and <= 1 => 0,
            < -1 and >= -2 => input + 1,
            > 1 and <= 2 => input - 1,
            _ => throw new ArgumentOutOfRangeException("Input is out of range (-2 to 2).")
        };
    }

    
    // Start is called before the first frame update
    void Start()
    {
        currentState = _boredState;
        currentState.EnterState(this);

        stationaryEyes = true;
        StartCoroutine(Blinking());
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);

        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(HeadTurnTest());
        }
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

    public void ChooseLookPoint(float x, float y)
    {
        stationaryEyes = false;
        if (_lookingCoroutine != null)
        {
            StopCoroutine(_lookingCoroutine);
        }
        _lookingCoroutine = StartCoroutine(LerpToLookPoint(x, y));
    }

    private IEnumerator LerpToLookPoint(float x, float y)
    {
        var elapsedTime = 0f;
        currentX = live2DModel.Parameters[1].Value;
        currentY = live2DModel.Parameters[2].Value;

        while (elapsedTime <= lookingSpeed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / lookingSpeed);
            
            var preValue = InOutCubic(normalizedTime);

            var valueX = Mathf.Lerp(currentX, x, preValue);
            var valueY = Mathf.Lerp(currentY, y, preValue);
            
            live2DModel.Parameters[1].Value = valueX;
            live2DModel.Parameters[2].Value = valueY;
            
            yield return null;
        }

        StartCoroutine(HeadTurn());
        
        stationaryEyes = true;
        
        currentX = live2DModel.Parameters[1].Value;
        currentY = live2DModel.Parameters[2].Value;
        
        while (stationaryEyes)
        {
            var dartingSpeed = Random.Range(0.8f, 3.5f);
            
            yield return new WaitForSeconds(dartingSpeed);
            live2DModel.Parameters[1].Value = currentX + Random.Range(-0.06f, 0.06f);
            live2DModel.Parameters[2].Value = currentY + Random.Range(-0.06f, 0.06f);
            
            yield return null;
        }
    }

    private static float InCubic(float t) => t * t * t;
    private static float OutCubic(float t) => 1 - InCubic(1 - t);
    private static float InOutCubic(float t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    private IEnumerator HeadTurn()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        
        var elapsedTime = 0f;
        var currentHeadRotation = live2DModel.Parameters[27].Value;
        
        while (elapsedTime <= turnSpeed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / turnSpeed);
            var preValueX = InOutCubic(normalizedTime);
            var valueX = Mathf.Lerp(currentHeadRotation, targetHeadX(live2DModel.Parameters[1].Value), preValueX);

            live2DModel.Parameters[27].Value = valueX;

            yield return null;
        }
    }


    private IEnumerator HeadTurnTest()
    {
        var elapsedTime = 0f;
        
        var currentHeadRotation = live2DModel.Parameters[27].Value;
        float targetRotation;
        if (currentHeadRotation < 0)
        {
            targetRotation = 0;
        }
        else
        {
            targetRotation = -1;
        }

        while (elapsedTime <= turnSpeed)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / turnSpeed);
            var preValueX = InOutCubic(normalizedTime);
            var valueX = Mathf.Lerp(currentHeadRotation, targetRotation, preValueX);

            live2DModel.Parameters[27].Value = valueX;

            yield return null;
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
    
}
