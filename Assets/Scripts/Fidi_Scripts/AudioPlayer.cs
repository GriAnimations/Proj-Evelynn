using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip audioClip;

    private int sampleRate = 24000;

    private List<float> audioBuffer = new();
    
    private bool startedPlaying = false;

    private Coroutine checkIfAudioDoneCoroutine;

    public delegate void OnAudioDoneDelegate();

    public event OnAudioDoneDelegate OnAudioDone;

    public delegate void OnAudioReadyDelegate(AudioClip clip);

    public event OnAudioReadyDelegate OnAudioReady;

    public bool Initialize()
    {
        try
        {
            audioSource = GetComponent<AudioSource>();

            checkIfAudioDoneCoroutine = StartCoroutine(CheckIfAudioDone());
        }
        catch (Exception e)
        {
            Debug.LogError("audio player error: " + e.Message);
            return false;
        }

        Debug.LogWarning("AudioPlayer initialized");
        return true;
    }

    public void Dispose()
    {
        StopCoroutine(checkIfAudioDoneCoroutine);
        audioSource.Stop();
        audioClip = null;
    }


    private IEnumerator CheckIfAudioDone()
    {
        while (true)
        {
            if (startedPlaying && audioSource.isPlaying == false && audioClip)
            {
                audioClip = null;
                startedPlaying = false;
                OnAudioDone?.Invoke();
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public void OnAudioDeltaMessage(string result)
    {
        byte[] audioBytes = Convert.FromBase64String(result);
        float[] samples = Utility.PCMToFloat(audioBytes);

        audioBuffer.AddRange(samples);
    }

    public void SetupAudio(string result)
    {
        audioClip = AudioClip.Create("AssistantAudio", audioBuffer.Count, 1, sampleRate, false);
        audioClip.SetData(audioBuffer.ToArray(), 0);
        
        audioBuffer.Clear();
        
        OnAudioReady?.Invoke(audioClip);
    }

    public void PlayAudio()
    {
        audioSource.clip = audioClip;
        
        SaveWav.Save(Guid.NewGuid().ToString(), audioClip);
        
        audioSource.Play();
        startedPlaying = true;
    }
}