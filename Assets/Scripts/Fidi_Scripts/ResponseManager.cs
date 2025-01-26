using System;
using System.Collections;
using System.Threading.Tasks;
using Fidi_Scripts;
using OpenAI.RealtimeConversation;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(SocketClient))]
[RequireComponent(typeof(AudioPlayer))]
[RequireComponent(typeof(AudioRec))]
[RequireComponent(typeof(OpenAiConnection))]
[RequireComponent(typeof(ConversationLogger))]
public class ResponseManager : MonoBehaviour
{
    private SocketClient socketClient;

    [SerializeField] private EmotionManager emotionManager;
    [SerializeField] private LookingStateManager lookingStateManager;

    private OpenAiConnection openAiConnection;

    private AudioPlayer audioPlayer;
    private AudioRec audioRecorder;
    private ConversationLogger conversationLogger;

    private AudioClip currentClip;

    [Header("Variables and Thresholds")] [SerializeField]
    private float longSentenceThreshold = 5;

    private float currentDuration = 0;

    private bool isAudioReady = false;
    private bool responseReady;
    private bool agentResponseReady;

    private Coroutine initCoroutine;

    private bool _longAnswer;

    private bool _firstStart;
    private bool _allowedToSpeak;
    private bool _pressedDown;

    [Header("Responses ")] [SerializeField]
    private string response = "";

    [SerializeField] private string agentResponse = "";

    [SerializeField] private string understoodText = "";

    private void Awake()
    {
        audioRecorder = GetComponent<AudioRec>();
        audioPlayer = GetComponent<AudioPlayer>();
        socketClient = GetComponent<SocketClient>();
        openAiConnection = GetComponent<OpenAiConnection>();
        conversationLogger = GetComponent<ConversationLogger>();
    }

    private void Start()
    {
        StartSession();
    }

    private void Update()
    {
        if (!_allowedToSpeak) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartMicrophone();
            _pressedDown = true;
            if (audioRecorder.manualMicrophone)
            {
                audioRecorder.StartSendingMode();
            }

            if (!_firstStart)
            {
                _firstStart = true;
                lookingStateManager.StopSleeping();
            }
        }

        if (audioRecorder.manualMicrophone)
        {
            if (Input.GetKeyUp(KeyCode.Space) && _pressedDown)
            {
                _pressedDown = false;
                StopMicrophone();
            }
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
        _allowedToSpeak = true;
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

    private IEnumerator WaitForAllReady()
    {
        Debug.LogWarning("Waiting for all to be ready");
        while (!(isAudioReady && responseReady && agentResponseReady))
        {
            Debug.LogWarning("Audio ready: " + isAudioReady);
            Debug.LogWarning("Response ready: " + responseReady);
            Debug.LogWarning("Agent response ready: " + agentResponseReady);
            yield return new WaitForSeconds(0.4f);
        }

        isAudioReady = false;
        responseReady = false;
        agentResponseReady = false;

        Debug.LogWarning("All ready");

        string userName = "Evelynn";

        int id = conversationLogger.AddMessage(userName, response, agentResponse);

        conversationLogger.SaveWavFile(currentClip, id, userName);
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
        if (conversationLogger.logging)
            conversationLogger.StartConversation();
        try
        {
            audioRecorder.OnSilenceDetected += StopMicAndSendData;
            if (audioRecorder.recognizer != null)
                audioRecorder.recognizer.OnPhraseRecognized += audioRecorder.OnPhraseRecognized;

            audioRecorder.OnTalkingDetected += audioRecorder.StartSendingMode;

            socketClient.OnTextDoneMessage += SetupText;

            socketClient.OnTextDoneMessage += text =>
            {
                response = text;
                responseReady = true;
            };

            socketClient.OnVoiceTranscriptDoneMessage += text => { understoodText = text; };
            if (conversationLogger.logging)
            {
                socketClient.OnVoiceTranscriptDoneMessage += OnResponseDone;
            }


            socketClient.OnAudioDoneMessage += audioPlayer.SetupAudio;
            socketClient.OnAudioDeltaMessage += audioPlayer.OnAudioDeltaMessage;

            audioPlayer.OnAudioReady += OnAudioReady;
            audioPlayer.OnAudioDone += () => isAudioReady = true;
            audioPlayer.OnAudioReady += clip => { currentClip = clip; };

            audioPlayer.OnAudioDone += OnAudioDone;

            openAiConnection.OnJobDone += CalculateFacsStuff;

            openAiConnection.OnJobDone += job =>
            {
                agentResponse = job.result;
                agentResponseReady = true;
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

        if (conversationLogger.logging)
        {
            socketClient.OnVoiceTranscriptDoneMessage -= OnResponseDone;
        }


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
        
        if (lookingStateManager.AsleepState.FranticLookAround)
        {
            lookingStateManager.SwitchState(lookingStateManager.ListeningState);
        }
    }

    public void StopMicrophone()
    {
        _allowedToSpeak = false;
        
        StopMicAndSendData();
        
        if (lookingStateManager.AsleepState.FranticLookAround)
        {
            lookingStateManager.SwitchState(lookingStateManager.ThinkingState);
        }
        
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
        audioRecorder.StopRecordingMode();
        audioRecorder.StopSendingMode();
        socketClient.CommitAudioData();
    }

    private void OnResponseDone(string text)
    {
        if (conversationLogger.started)
        {
            Debug.Log("logging");
            string userName = "User";

            int id = conversationLogger.AddMessage(userName, text);

            AudioClip clip = AudioClip.Create("AssistantAudio", audioRecorder.sentBytes.Count, 1,
                audioRecorder.sampleRate, false);
            clip.SetData(audioRecorder.sentBytes.ToArray(), 0);


            conversationLogger.SaveWavFile(clip, id, userName);

            audioRecorder.sentBytes?.Clear();

            responseReady = false;
            agentResponseReady = false;
            isAudioReady = false;

            StartCoroutine(WaitForAllReady());
        }
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
            lookingStateManager.SwitchState(lookingStateManager.TalkingState);
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
        lookingStateManager.SwitchState(lookingStateManager.AttentionState);
        _allowedToSpeak = true;
    }

    private void CalculateFacsStuff((Guid id, string result) job)
    {
        float useThisShit = currentDuration; //TODO USE THAT SHIT 
        string result = job.result; //TODO USE THIS SHIT AS WELL

        JsonReturn jsonReturn = JasonDecoder.DecodeJason(result);

        emotionManager.StartNewEmotion(jsonReturn, useThisShit, response);

        if (_longAnswer)
        {
            LongSentence(currentDuration);
            lookingStateManager.SwitchState(lookingStateManager.TalkingState);
        }

        //JasonDecoder.DecodeJason("asd");
        //JsonReturn jsonReturn = JsonUtility.FromJson<JsonReturn>(job.result);

        //TODO CALCULATE WHEN WHAT FACS SHOULD BE SHOWN
    }

    public void StartResetSession()
    {
        StartCoroutine(ResetSession());
    }

    private IEnumerator ResetSession()
    {
        _allowedToSpeak = false;
        
        lookingStateManager.SwitchState(lookingStateManager.AsleepState);
        _firstStart = false;

        yield return new WaitForSeconds(1f);
        
        //StopSession();
        yield return new WaitForSeconds(5f);
        //StartSession();
        _allowedToSpeak = true;
    }
}