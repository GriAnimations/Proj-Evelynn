using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using LookingStateMachine;
using UnityEngine;

public class BodyLanguage : MonoBehaviour
{
    public CubismModel live2DModel;
    [SerializeField] private EmotionManager emotionManager;
    [SerializeField] private LookingStateManager lookingStateManager;

    public float finalHeadX;
    public float finalHeadY;
    public float finalBodyX;
    
}
