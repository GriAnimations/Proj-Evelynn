using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RadioManager : MonoBehaviour
{
    private static readonly int Property = Shader.PropertyToID("_DispIntensity");
    private static readonly int Property1 = Shader.PropertyToID("_ColorIntensity");
    public float radioChannel;

    [SerializeField] private AudioSource whiteNoise;
    [SerializeField] private AudioSource channel1;
    [SerializeField] private AudioSource channel2;
    [SerializeField] private AudioSource channel3;
    [SerializeField] private AudioClip[] musicClips1;
    [SerializeField] private AudioClip[] musicClips2;
    [SerializeField] private AudioClip[] musicClips3;

    [SerializeField] private Slider channelSlider;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private float[] crossFadePoints;
    [SerializeField] private float crossFadeThreshold = 0.15f;
    public float maxWhiteNoiseVolume = 0.2f;

    [SerializeField] private Material radioImageMat;

    private int channel1Index = 0;
    private int channel2Index = 0;
    private int channel3Index = 0;

    private int _currentClipIndex;

    private SocketClient _socketClient;

    private void Start()
    {
        volumeSlider.onValueChanged.AddListener(v => volumeSlider.value = v);
        StartCoroutine(VolumeUp());
    }

    private void Update()
    {
        if (Time.frameCount % 10 != 0) return;

        ChannelControl();
        CheckAndAdvanceChannel(channel1, musicClips1, ref channel1Index);
        CheckAndAdvanceChannel(channel2, musicClips2, ref channel2Index);
        CheckAndAdvanceChannel(channel3, musicClips3, ref channel3Index);
    }

    private IEnumerator VolumeUp()
    {
        yield return new WaitForSeconds(5f);
        float elapsedTime = 0;
        while (elapsedTime <= 3f)
        {
            elapsedTime += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsedTime / 3f);
            var preValue = Mathf.Lerp(0, 0.4f, normalizedTime);

            volumeSlider.value = preValue;

            yield return null;
        }
    }

    public void ChannelControl()
    {
        var activations = new float[3];
        var maxActivation = 0f;
        for (var i = 0; i < 3; i++)
        {
            var distance = Mathf.Abs(channelSlider.value - crossFadePoints[i]);
            var activation = Mathf.Clamp01(1f - distance / crossFadeThreshold);
            activations[i] = activation;
            if (activation > maxActivation)
                maxActivation = activation;
        }

        channel1.volume = activations[0] * volumeSlider.value;
        channel2.volume = activations[1] * volumeSlider.value;
        channel3.volume = activations[2] * volumeSlider.value;
        whiteNoise.volume = Mathf.Lerp(maxWhiteNoiseVolume, -0.5f, maxActivation);

        radioImageMat.SetFloat(Property, whiteNoise.volume * 2);
        radioImageMat.SetFloat(Property1, whiteNoise.volume * 2);
    }

    void CheckAndAdvanceChannel(AudioSource source, AudioClip[] clips, ref int index)
    {
        if (source.isPlaying) return;
        index = (index + 1) % clips.Length;
        source.clip = clips[index];
        source.Play();
    }
}