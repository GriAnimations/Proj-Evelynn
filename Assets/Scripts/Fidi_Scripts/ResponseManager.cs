using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(SocketClient))]
[RequireComponent(typeof(AudioPlayer))]
[RequireComponent(typeof(AudioRec))]
[RequireComponent(typeof(OpenAiConnection))]
public class ResponseManager : MonoBehaviour
{ 
    private SocketClient socketClient;
    
    [SerializeField] private EmotionManager emotionManager;
    
    private OpenAiConnection openAiConnection;

    private AudioPlayer audioPlayer;
    private AudioRec audioRecorder;

    [Header("Variables and Thresholds")] [SerializeField]
    private float longSentenceThreshold = 5;

    private float currentDuration = 0;

    private bool isAudioReady = false;

    private Coroutine initCoroutine;

    private bool _longAnswer;
    
    [Header("Responses ")]
    
    [SerializeField] private string response = "";
    
    [SerializeField] private string agentResponse = "";
    
    [SerializeField] private string understoodText = "";
    
    
    
    /*private class SessionHistory
    {
    }

    private class Session
    {
        string id = Guid.NewGuid().ToString();


    }

    private class


    private enum Role
    {
        User,
        Agent,
        Setting
    }*/

    private void Awake()
    {
        audioRecorder = GetComponent<AudioRec>();
        audioPlayer = GetComponent<AudioPlayer>();
        socketClient = GetComponent<SocketClient>();
        openAiConnection = GetComponent<OpenAiConnection>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning("Session starting");
            StartSession();
            Debug.LogWarning("Session started");

            //StartMicrophone();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.LogWarning("Session stopping");
            StopSession();
            Debug.LogWarning("Session stopped");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            StartMicrophone();
        }
    }


    private void StartSession()
    {
        StartCoroutine(InitializeSessionsAsync(socketClient.ConnectAsync, socketClient.Initialize, StartListeners));
        StartCoroutine(InitializeSessions(audioRecorder.Initialize, audioPlayer.Initialize, openAiConnection.Initialize));
    }

    private IEnumerator InitializeSessionsAsync(Func<Task> initFunc, params Func<bool>[] executeAfter)
    {
        while (true)
        {
            Task task = initFunc();

            while (!task.IsCompleted)
            {
                Debug.LogWarning("Task not done yet");
                yield return new WaitForSeconds(0.2f);
            }

            if (task.IsCompletedSuccessfully)
            {
                foreach (var action in executeAfter)
                {
                    if (!action())
                    {
                        Debug.LogError("Failed to initialize, Fucking off...");
                        throw new Exception("Failed to initialize");
                    }
                    yield return null;
                }
                break;
            }

            Debug.LogError("Failed to initialize, trying again...");
        }
        Debug.LogWarning("Initialized");
    }

    private IEnumerator InitializeSessions(params Func<bool>[] initFuncs)
    {
        foreach (var func in initFuncs)
        {
            while (!func())
            {
                Debug.LogError("Failed to initialize, trying again...");
                yield return new WaitForSeconds(0.1f);
            }

            yield return null;
        }
        
        Debug.LogWarning("Initialized");
    }


    private void StopSession()
    {
        StopListeners();

        audioRecorder.Dispose();
        audioPlayer.Dispose();
        openAiConnection.Dispose();
        socketClient.Disconnect();
    }

    private bool StartListeners()
    {
        try
        {
            audioRecorder.OnSilenceDetected += StopMicAndSendData;
            if (audioRecorder.recognizer != null)
                audioRecorder.recognizer.OnPhraseRecognized += audioRecorder.OnPhraseRecognized;
            
            audioRecorder.OnTalkingDetected += audioRecorder.StartSendingMode;

            socketClient.OnTextDoneMessage += SetupText;
            
            socketClient.OnTextDoneMessage += (text) =>
            {
                response = text;
            };
            
            socketClient.OnVoiceTranscriptDoneMessage += (text) =>
            {
                understoodText = text;
            };
            
            socketClient.OnAudioDoneMessage += audioPlayer.SetupAudio;
            socketClient.OnAudioDeltaMessage += audioPlayer.OnAudioDeltaMessage;

            audioPlayer.OnAudioReady += OnAudioReady;
            audioPlayer.OnAudioDone += OnAudioDone;

            openAiConnection.OnJobDone += CalculateFacsStuff;
            
            openAiConnection.OnJobDone += (job) =>
            {
                agentResponse = job.result;
            };
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }

        return true;
    }

    private void StopListeners()
    {
        audioRecorder.OnSilenceDetected -= StopMicAndSendData;
        if (audioRecorder.recognizer != null)
            audioRecorder.recognizer.OnPhraseRecognized -= audioRecorder.OnPhraseRecognized;

        socketClient.OnTextDoneMessage -= SetupText;
        socketClient.OnAudioDoneMessage -= audioPlayer.SetupAudio;
        socketClient.OnAudioDeltaMessage -= audioPlayer.OnAudioDeltaMessage;
        

        audioPlayer.OnAudioReady -= OnAudioReady;
        audioPlayer.OnAudioDone -= OnAudioDone;

        openAiConnection.OnJobDone -= CalculateFacsStuff;
    }


    //Step 1 StartMicrophone()
    //Step 2 OnSilenceDetected -> StopMicAndSendData()
    //Step 3 OnTextDoneMessage -> SetupText()
    //Step 4 OnJobDone && OnAudioReady -> CalculateFacsStuff()
    //Step 5 PlayAudio()
    //Step 6 OnAudioDone -> StartMicrophone()


    public void StartMicrophone()
    {
        audioRecorder.StartRecordingMode();
    }

    private void SetupText(string result)
    {
        Guid resultGuid = Guid.NewGuid();
        Debug.Log("Text: " + result);
        openAiConnection.AddJob(result, resultGuid);

        //TODO SEND TEXT TO OPENAI CHATCLIENT
    }

    private void StopMicAndSendData()
    {
        Debug.LogWarning("Silence detected");
        audioRecorder.StopRecordingMode();
        audioRecorder.StopSendingMode();
        socketClient.CommitAudioData();
    }

    private void OnAudioReady(AudioClip clip)
    {
        _longAnswer = false;
        
        currentDuration = clip.length;
        if (clip.length > longSentenceThreshold)
        {
            _longAnswer = true;
        }
        else
        {
            ShortSentence(currentDuration);
        }
    }

    private void ShortSentence(float duration)
    {
        audioPlayer.PlayAudio();
        //TODO Whatever you want to do with short sentences
        Debug.Log("Short sentence");
    }

    private void LongSentence(float duration)
    {
        audioPlayer.PlayAudio(); // DO OTHER SHIT BEFORE THIS

        //TODO Whatever you want to do with long sentences
        Debug.Log("Long sentence");
    }


    private void OnAudioDone()
    {
        StartMicrophone();
    }

    private void CalculateFacsStuff((Guid id, string result) job)
    {
        float useThisShit = currentDuration; //TODO USE THAT SHIT 
        string result = job.result; //TODO USE THIS SHIT AS WELL
        
        JsonReturn jsonReturn = JasonDecoder.DecodeJason(result);
        
        emotionManager.StartNewEmotion(jsonReturn);

        if (_longAnswer)
        {
            LongSentence(currentDuration);
        }

        //JasonDecoder.DecodeJason("asd");
        //JsonReturn jsonReturn = JsonUtility.FromJson<JsonReturn>(job.result);
        
        //TODO CALCULATE WHEN WHAT FACS SHOULD BE SHOWN
    }
}