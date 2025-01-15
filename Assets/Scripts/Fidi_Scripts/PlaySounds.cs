using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(AudioSource))]
    public class PlaySounds : MonoBehaviour
    {
        
        private AudioSource audioSource;
        private List<float> audioDataBuffer = new();

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public void PlayAudio(byte[] audioBytes)
        {
            // Convert 16-bit PCM bytes to float samples
            float[] samples = Utility.PCMToFloat(audioBytes);

            // Add samples to buffer
            audioDataBuffer.AddRange(samples);

            // Create AudioClip and play
            AudioClip clip = AudioClip.Create("AssistantAudio", audioDataBuffer.Count, 1, 24000, false);
            clip.SetData(audioDataBuffer.ToArray(), 0);

            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                // Update clip data while playing
                audioSource.clip = clip;
            }
        }
        
        
    }
}