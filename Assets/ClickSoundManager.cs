using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    public void PlayClickSound(int clip)
    {
        audioSource.PlayOneShot(clips[clip]);
    } 
}
