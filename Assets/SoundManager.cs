using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private AudioClip[] clips;

    public void PlaySound(int clip)
    {
        audioSource.PlayOneShot(clips[clip]);
    }
    
    public void PlayClickSound(int clip)
    {
        clickAudioSource.PlayOneShot(clips[clip]);
    } 
}
