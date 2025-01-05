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
    //public List<uLipSyncBlendShape.BlendShapeInfo> blendShapes;

    [SerializeField] private float smoothTime;

    private string _currentVowel;
    private int _correspondingIndex;

    [SerializeField] private int[] mouthIndexList;
    
    public class BlendShapeInfo
    {
        public string phoneme;
        public int index = -1;
        public float maxWeight = 1f;

        public float weight { get; set; } = 0f;
        public float weightVelocity { get; set; } = 0f;
    }
    
    public List<BlendShapeInfo> blendShapes = new List<BlendShapeInfo>();
    
    LipSyncInfo _info = new LipSyncInfo();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateMouth(_correspondingIndex);
    }

    private void UpdateMouth(int index)
    {
        //ResetIndexValue();
        live2DModel.Parameters[index].Value = _info.volume;
    }
    
    public void OnLipSyncUpdate(LipSyncInfo info)
    {
        _info = info;
        
        ChooseCorrectIndex();
        //live2DModel.Parameters[_correspondingIndex].Value = 1;
    }

    private void ResetIndexValue()
    {
        foreach (var x in mouthIndexList)
        {
            live2DModel.Parameters[x].Value = 0;
        }
    }

    private void ChooseCorrectIndex()
    {
        _correspondingIndex = _info.phoneme switch
        {
            "A" => 26,
            "E" => 27,
            "I" => 28,
            "O" => 29,
            "U" => 30,
            "SH" => 32,
            _ => _correspondingIndex
        };
    }
}
