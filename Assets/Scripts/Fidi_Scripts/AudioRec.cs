using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

public class AudioRec : MonoBehaviour
{
    AudioClip audioClip;
    public int sampleRate = 24000; // Standard sample rate
    int chunkSize = 8192; // You can adjust this as needed for processing
    private string microphoneName;

    [Header("Turn detection")] [SerializeField]
    private float threshold = 0.001f;

    public bool manualMicrophone = true;


    [SerializeField] private float silenceDuration = 1;
    [SerializeField] private float talkDuration = 0.5f;

    private float bufferSize = 10;

    private int lastSampleOffset = 0;

    private Coroutine detectNotTalkingCoroutine;
    private Coroutine detectTalkingCoroutine;

    private List<float> audioBuffer;

    private SocketClient socketClient;

    [SerializeField] private float currentAmplitude;

    private AudioPlayer audioPlayer;

    public delegate void OnAudioDelegate();

    public event OnAudioDelegate OnSilenceDetected;
    public event OnAudioDelegate OnTalkingDetected;

    public event OnFloatChangeDelegate OnCurrentAmplitudeChange;

    public delegate void OnBoolChangeDelegate(bool value);

    public delegate void OnFloatChangeDelegate(float value);

    public event OnBoolChangeDelegate OnRecordingModeChange;

    public event OnBoolChangeDelegate OnSilenceChange;

    [SerializeField] private bool silence = true;

    [SerializeField] public List<float> sentBytes = new();


    public bool Silence
    {
        get => silence;
        set
        {
            silence = value;
            OnSilenceChange?.Invoke(silence);
        }
    }

    //[SerializeField] private List<float> talkingSamples = new();

    [SerializeField] private bool recordingMode = false;

    [SerializeField] private bool sendingMode = false;

    public bool RecordingMode
    {
        get => recordingMode;
        set
        {
            recordingMode = value;
            OnRecordingModeChange?.Invoke(recordingMode);
        }
    }

    public KeywordRecognizer recognizer;

    [Header("Keyword detection")] [SerializeField]
    private List<ActionKeyWordPair> actionKeyWordPairs = new();

    private Dictionary<string, UnityEvent<ConfidenceLevel>> actionKeyWordDict = new();

    public void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        actionKeyWordDict[args.text]?.Invoke(args.confidence);
    }

    private void Awake()
    {
        socketClient = GetComponent<SocketClient>();
        audioPlayer = GetComponent<AudioPlayer>();
    }

    public bool Initialize()
    {
        try
        {
            microphoneName = Microphone.devices[0];
            //audioBuffer = new CircularBuffer<float>((int)(sampleRate * (bufferSize)));
            audioBuffer = new List<float>();
            StartMicrophone();

            if (actionKeyWordPairs.Count > 0)
            {
                recognizer = new KeywordRecognizer(actionKeyWordPairs.Select(pair => pair.KeyWord).ToArray());

                foreach (var pair in actionKeyWordPairs)
                {
                    actionKeyWordDict.Add(pair.KeyWord, pair.Event);
                }
            }
            else
            {
                Debug.LogWarning("No action key word pairs found");
            }

            StartCoroutine(SendAudioChunks());

            if (!manualMicrophone)
            {
                detectNotTalkingCoroutine = StartCoroutine(DetectTalkingAndSilence());
            }
        }
        catch (Exception e)
        {
            Debug.LogError("AudioRecorder error: " + e.Message);
            return false;
        }

        Debug.LogWarning("AudioRecorder initialized");
        return true;
    }

    public void Dispose()
    {
        StopRecording();
        StopRecordingMode();
        audioBuffer?.Clear();

        StopCoroutine(detectNotTalkingCoroutine);
    }

    public void StartRecordingMode()
    {
        audioBuffer?.Clear();
        RecordingMode = true;
        recognizer?.Start();
    }

    public void StartSendingMode()
    {
        sentBytes?.Clear();
        audioBuffer?.Clear();
        sendingMode = true;
    }

    public void StopSendingMode()
    {
        sendingMode = false;
    }

    public void StopRecordingMode()
    {
        RecordingMode = false;
        recognizer?.Stop();
    }

    private void StartMicrophone()
    {
        Debug.LogWarning("Starting microphone...");
        audioClip = Microphone.Start(microphoneName, true, 5, sampleRate);
        Debug.LogWarning("Microphone started...");
    }

    IEnumerator SendAudioChunks()
    {
        while (true)
        {
            while (Microphone.IsRecording(microphoneName))
            {
                yield return new WaitForSeconds(0.1f); // Was 0.2 TODO
                if (RecordingMode)
                {
                    currentAmplitude = GetAverageAmplitude(0.1f);

                    float[] currentAudioBuffer = GetAudioDelta();

                    byte[] sampleBytes = Utility.FloatToPCM(currentAudioBuffer);

                    string audioBytes = Convert.ToBase64String(sampleBytes);

                    if (sendingMode)
                    {
                        /*if (talkingSamples?.Count > 0)
                        {
                            byte[] talkingSampleBytes = Utility.FloatToPCM(talkingSamples.ToArray());
                            string talkingAudioBytes = Convert.ToBase64String(talkingSampleBytes);
                            audioBytes = talkingAudioBytes + audioBytes;
                            talkingSamples?.Clear();
                        }
                        */

                        sentBytes.AddRange(currentAudioBuffer);

                        socketClient.AddAudioToQueue(audioBytes);
                    }
                }
            }

            Debug.LogWarning("Waiting for audio...");
            yield return new WaitForSeconds(0.5f);
        }
    }

    void StopRecording()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            Debug.LogWarning("Recording stopped...");
        }
    }

    private IEnumerator DetectTalkingAndSilence()
    {
        while (true)
        {
            if (RecordingMode)
            {
                if (Silence)
                {
                    float duration;
                    float startTime = Time.time;

                    while (currentAmplitude > threshold && Silence)
                    {
                        duration = Time.time - startTime;

                        if (duration > talkDuration)
                        {
                            Silence = false;
                            /*talkingSamples = audioBuffer.GetRange(audioBuffer.Count - (int)((duration + 0.1f) * sampleRate),
                                (int)((duration + 0.1f) * sampleRate));*/
                            OnTalkingDetected?.Invoke();
                        }

                        yield return null;
                    }
                }

                else
                {
                    float duration;
                    float startTime = Time.time;

                    while (currentAmplitude < threshold && !Silence)
                    {
                        duration = Time.time - startTime;

                        if (duration > silenceDuration)
                        {
                            Silence = true;
                            OnSilenceDetected?.Invoke();
                        }

                        yield return null;
                    }
                }
            }

            yield return null;
        }
    }

    private float[] GetAudioDelta()
    {
        int currentSampleOffset = Microphone.GetPosition(microphoneName);

        if (currentSampleOffset < lastSampleOffset)
        {
            if (currentSampleOffset == 0)
            {
                int chunkCount = audioClip.samples - lastSampleOffset;
                float[] chunk = new float[chunkCount];
                audioClip.GetData(chunk, lastSampleOffset);
                lastSampleOffset = currentSampleOffset;

                audioBuffer.AddRange(chunk);
                return chunk;
            }

            int firstChunkSize = audioClip.samples - lastSampleOffset;
            int secondChunkSize = currentSampleOffset;
            float[] firstChunk = new float[firstChunkSize];
            float[] secondChunk = new float[secondChunkSize];

            audioClip.GetData(firstChunk, lastSampleOffset);
            audioClip.GetData(secondChunk, 0);

            lastSampleOffset = currentSampleOffset;

            float[] audioDelta = new float[firstChunkSize + secondChunkSize];
            Array.Copy(firstChunk, 0, audioDelta, 0, firstChunkSize);
            Array.Copy(secondChunk, 0, audioDelta, firstChunkSize, secondChunkSize);

            audioBuffer.AddRange(audioDelta);

            return audioDelta;
        }
        else
        {
            float[] audioDelta = new float[currentSampleOffset - lastSampleOffset];
            audioClip.GetData(audioDelta, lastSampleOffset);
            lastSampleOffset = currentSampleOffset;

            audioBuffer.AddRange(audioDelta);
            return audioDelta;
        }
    }

    private bool IsAmplitudeBelowThreshold(float duration)
    {
        if (audioBuffer.Count < duration * sampleRate)
        {
            return false;
        }

        Debug.Log(audioBuffer.Count);

        float[] samples = audioBuffer
            .GetRange(audioBuffer.Count - (int)(duration * sampleRate), (int)(duration * sampleRate)).ToArray();

        float averageAmplitude = samples.Average();

        return averageAmplitude < threshold;
    }

    private bool IsAmplitudeAboveThreshold(float duration)
    {
        if (audioBuffer.Count < duration * sampleRate)
        {
            return false;
        }

        Debug.Log(audioBuffer.Count);

        float[] samples = audioBuffer
            .GetRange(audioBuffer.Count - (int)(duration * sampleRate), (int)(duration * sampleRate)).ToArray();

        float averageAmplitude = samples.Average();

        return averageAmplitude > threshold;
    }

    private float GetAverageAmplitude(float duration)
    {
        if (audioBuffer.Count < duration * sampleRate)
        {
            return 0;
        }

        float[] samples = audioBuffer
            .GetRange(audioBuffer.Count - (int)(duration * sampleRate), (int)(duration * sampleRate)).ToArray();

        return samples.Average();
    }
}

[Serializable]
public struct ActionKeyWordPair
{
    public string KeyWord;
    public UnityEvent<ConfidenceLevel> Event;
}