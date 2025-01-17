using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using uLipSync;
using UnityEngine;

public class LipSync : MonoBehaviour
{
    [SerializeField] private CubismModel live2DModel;
    [SerializeField] private CubismParametersInspector parameters;
  
    private int _correspondingIndex;

    [SerializeField] private int[] mouthIndexList;
    
    private int[] _currentMouthIndexList = new int[18];

    private float _smoothedVolume;
    private const float MaxIncreasePerFrame = 0.03f;
    private const float SmoothingFactor = 0.1f;

    private int _lastIndex;

    private LipSyncInfo _info;

    private float SmoothVolume(float targetVolume)
    {
        if (targetVolume > _smoothedVolume)
        {
            _smoothedVolume += Math.Min(targetVolume - _smoothedVolume, MaxIncreasePerFrame);
        }
        else
        {
            _smoothedVolume += (targetVolume - _smoothedVolume) * SmoothingFactor;
        }
        
        _smoothedVolume = Math.Clamp(_smoothedVolume, 0f, 1f);

        return _smoothedVolume;
    }
    

    void LateUpdate()
    {
        //UpdateMouth(_correspondingIndex);
    }

    private void UpdateMouth(int index)
    {
        if (_lastIndex != index)
        {
            _smoothedVolume = 0;
        }
        
        live2DModel.Parameters[index].Value += SmoothVolume(_info.volume);
        
        _lastIndex = index;
    }
    
    public void OnLipSyncUpdate(LipSyncInfo info)
    {
        _info = info;
        ChooseCorrectIndex();

        if (_lastIndex != _correspondingIndex)
        {
            _currentMouthIndexList[_correspondingIndex]++;
            StartCoroutine(LipSyncCoroutine(0.1f, _correspondingIndex));
        }
        _lastIndex = _correspondingIndex;
    }

    private void ChooseCorrectIndex()
    {
        _correspondingIndex = _info.phoneme switch
        {
            "Dsch" => mouthIndexList[0],
            "UE" => mouthIndexList[1],
            "OU" => mouthIndexList[2],
            "WO" => mouthIndexList[3],
            "U" => mouthIndexList[4],
            "S" => mouthIndexList[5],
            "Z" => mouthIndexList[6],
            "I" => mouthIndexList[7],
            "O" => mouthIndexList[8],
            "OE" => mouthIndexList[9],
            "F" => mouthIndexList[10],
            "W" => mouthIndexList[11],
            "M" => mouthIndexList[12],
            "AH" => mouthIndexList[13],
            "A" => mouthIndexList[14],
            "R" => mouthIndexList[15],
            "E" => mouthIndexList[16],
            "L" => mouthIndexList[17],
            _ => _correspondingIndex
        };
    }

    private IEnumerator LipSyncCoroutine(float duration, int index)
    {
        yield return null;
        
        var elapsedTime = 0f;
        var currentValue = live2DModel.Parameters[index].Value;
        var targetValue = _info.volume;

        while (elapsedTime <= duration)
        {
            if (_currentMouthIndexList[index] >= 2)
            {
                break;
            }
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            var easedTime = EasingFunctions.OutBack(normalizedTime);
            var value = currentValue + (targetValue - currentValue) * easedTime;
            
            live2DModel.Parameters[index].Value = value;
            
            yield return null;
        }
        
        elapsedTime = 0f;
        currentValue = live2DModel.Parameters[index].Value;
        
        while (elapsedTime <= duration)
        {
            if (_currentMouthIndexList[index] >= 2)
            {
                break;
            }
            
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            var easedTime = EasingFunctions.InSine(normalizedTime);
            var value = Mathf.Lerp(currentValue, 0, easedTime);
            
            live2DModel.Parameters[index].Value = value;
            
            yield return null;
        }
        
        _currentMouthIndexList[index]--;
    }
}
