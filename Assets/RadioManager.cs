using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RadioManager : MonoBehaviour
{
    public float radioChannel;

    [SerializeField] private AudioSource whiteNoise;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioClip[] musicClips;
    
    [SerializeField] private Slider channelSlider;
    [SerializeField] private Slider volumeSlider;
    
    [SerializeField] private float[] crossFadePoints;
    [SerializeField] private float crossFadeThreshold = 0.15f;
    
    //public float maxVolume;
    
    private int _currentClipIndex;

    private void Start()
    {
        volumeSlider.value = 0;
        
        volumeSlider.onValueChanged.AddListener(v => volumeSlider.value = v);
    }

    private void Update()
    {
        if (Time.frameCount % 10 == 0)
        {
            ChannelControl();
        }
    }

    public void ChannelControl()
    {
        var closestPointIndex = -1;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < crossFadePoints.Length; i++)
        {
            var distance = Mathf.Abs(channelSlider.value - crossFadePoints[i]);
            if (!(distance < closestDistance)) continue;
            closestDistance = distance;
            closestPointIndex = i;
        }
        
        var distanceToPoint = Mathf.Abs(channelSlider.value - crossFadePoints[closestPointIndex]);
        var t = Mathf.Clamp01(1 - distanceToPoint / crossFadeThreshold);

        whiteNoise.volume = Mathf.Lerp(volumeSlider.value/5, -0.1f, t);
        music.volume = Mathf.Lerp(0, volumeSlider.value, t);

        if (_currentClipIndex == closestPointIndex) return;
        _currentClipIndex = closestPointIndex;
        music.clip = musicClips[_currentClipIndex];
        music.Play();
    }
}
