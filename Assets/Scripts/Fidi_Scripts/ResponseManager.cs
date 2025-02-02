using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Fidi_Scripts;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(SocketClient))]
[RequireComponent(typeof(AudioPlayer))]
[RequireComponent(typeof(AudioRec))]
[RequireComponent(typeof(OpenAiConnection))]
[RequireComponent(typeof(ConversationLogger))]
public class ResponseManager : MonoBehaviour
{
    private static readonly int Status = Animator.StringToHash("status");
    public SocketClient socketClient;

    private EmotionManager emotionManager;
    private LookingStateManager lookingStateManager;
    private Button resetButton;
    private Animator tvAnimator;

    private OpenAiConnection openAiConnection;

    private AudioPlayer audioPlayer;
    private AudioRec audioRecorder;
    private ConversationLogger conversationLogger;

    private uLipSync.uLipSync uLipSync;

    private LipSync lipSync;

    private AudioClip currentClip;

    [Header("Variables and Thresholds")] [SerializeField]
    private float longSentenceThreshold = 5;

    private float currentDuration = 0;

    private bool isAudioReady = false;
    private bool responseReady = false;
    private bool agentResponseReady = false;

    private Coroutine initCoroutine;

    private bool _longAnswer;

    private bool _firstStart;
    public bool allowedToSpeak;
    private bool _pressedDown;

    [Header("Responses ")] [SerializeField]
    private string response = "";

    [SerializeField] private string agentResponse = "";

    [SerializeField] private string understoodText = "";

    private bool sessionStopped = true;

    private void Awake()
    {
        audioRecorder = GetComponent<AudioRec>();
        audioPlayer = GetComponent<AudioPlayer>();
        socketClient = GetComponent<SocketClient>();
        openAiConnection = GetComponent<OpenAiConnection>();
        conversationLogger = GetComponent<ConversationLogger>();

        emotionManager = FindObjectOfType<EmotionManager>();

        lookingStateManager = FindObjectOfType<LookingStateManager>();

        resetButton = Resources.FindObjectsOfTypeAll<Button>().First(button => button.name == "Reset_Button");

        tvAnimator = FindObjectsOfType<Animator>().First(animator => animator.name == "TV1");

        uLipSync = GetComponent<uLipSync.uLipSync>();
        lipSync = FindObjectOfType<LipSync>();
    }

    private void Start()
    {
        resetButton.interactable = false;
        StartSession();
    }

    private void Update()
    {
        if (!allowedToSpeak) return;

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

    public void ShutDownWarning()
    {
        socketClient.CommitAudioAndRequestResponse("You are being turned off now, act quickly");
    }


    private void OnDestroy()
    {
        StopSession();
    }

    private void StartSession()
    {
        StartCoroutine(InitializeSessionsAsync(socketClient.ConnectAsync, socketClient.Initialize, StartListeners));
        StartCoroutine(
            InitializeSessions(audioRecorder.Initialize, audioPlayer.Initialize, openAiConnection.Initialize));
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
        audioRecorder.Dispose();
        Debug.LogWarning("AudioRecorder disposed");
        audioPlayer.Dispose();
        Debug.LogWarning("AudioPlayer disposed");
        openAiConnection.Dispose();
        Debug.LogWarning("OpenAiConnection disposed");
        socketClient.Disconnect();
        Debug.LogWarning("SocketClient disposed");

        StopListeners();
    }

    private bool StartListeners()
    {
        if (conversationLogger.logging)
            conversationLogger.StartConversation();
        try
        {
            audioRecorder.OnSilenceDetected += StopMicAndSendData;

            if (audioRecorder.recognizer != null)
            {
                audioRecorder.recognizer.OnPhraseRecognized += audioRecorder.OnPhraseRecognized;
            }

            audioRecorder.OnTalkingDetected += audioRecorder.StartSendingMode;

            socketClient.OnTextDoneMessage += SetupText;

            socketClient.OnTextDoneMessage += OnSocketClientOnOnTextDoneMessage;

            socketClient.OnVoiceTranscriptDoneMessage += OnSocketClientOnOnVoiceTranscriptDoneMessage;

            if (conversationLogger.logging)
            {
                socketClient.OnVoiceTranscriptDoneMessage += OnResponseDone;
            }

            socketClient.OnAudioDoneMessage += audioPlayer.SetupAudio;
            socketClient.OnAudioDeltaMessage += audioPlayer.OnAudioDeltaMessage;

            audioPlayer.OnAudioReady += OnAudioReady;
            audioPlayer.OnAudioDone += OnAudioPlayerOnOnAudioDone;
            audioPlayer.OnAudioReady += OnAudioPlayerOnOnAudioReady;

            audioPlayer.OnAudioDone += OnAudioDone;

            openAiConnection.OnJobDone += CalculateFacsStuff;

            openAiConnection.OnJobDone += OnOpenAiConnectionOnOnJobDone;

            uLipSync.onLipSyncUpdate.AddListener(lipSync.OnLipSyncUpdate);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }

        sessionStopped = false;

        return true;
    }

    private void OnOpenAiConnectionOnOnJobDone((Guid id, string result) job)
    {
        agentResponse = job.result;
        agentResponseReady = true;
    }

    private void OnAudioPlayerOnOnAudioReady(AudioClip clip)
    {
        currentClip = clip;
    }

    private void OnAudioPlayerOnOnAudioDone()
    {
        isAudioReady = true;
    }

    private void OnSocketClientOnOnVoiceTranscriptDoneMessage(string text)
    {
        understoodText = text;
    }

    private void OnSocketClientOnOnTextDoneMessage(string text)
    {
        response = text;
        responseReady = true;
    }

    private void StopListeners()
    {
        audioRecorder.OnSilenceDetected -= StopMicAndSendData;

        if (audioRecorder.recognizer != null)
        {
            audioRecorder.recognizer.OnPhraseRecognized -= audioRecorder.OnPhraseRecognized;
        }

        audioRecorder.OnTalkingDetected -= audioRecorder.StartSendingMode;

        socketClient.OnTextDoneMessage -= SetupText;

        socketClient.OnTextDoneMessage -= OnSocketClientOnOnTextDoneMessage;

        socketClient.OnVoiceTranscriptDoneMessage -= OnSocketClientOnOnVoiceTranscriptDoneMessage;

        if (conversationLogger.logging)
        {
            socketClient.OnVoiceTranscriptDoneMessage -= OnResponseDone;
        }

        socketClient.OnAudioDoneMessage -= audioPlayer.SetupAudio;
        socketClient.OnAudioDeltaMessage -= audioPlayer.OnAudioDeltaMessage;

        audioPlayer.OnAudioReady -= OnAudioReady;
        audioPlayer.OnAudioDone -= OnAudioPlayerOnOnAudioDone;
        audioPlayer.OnAudioReady -= OnAudioPlayerOnOnAudioReady;

        audioPlayer.OnAudioDone -= OnAudioDone;

        openAiConnection.OnJobDone -= CalculateFacsStuff;

        openAiConnection.OnJobDone -= OnOpenAiConnectionOnOnJobDone;

        uLipSync.onLipSyncUpdate.RemoveListener(lipSync.OnLipSyncUpdate);

        sessionStopped = true;
    }


    //Step 1 StartMicrophone()
    //Step 2 OnSilenceDetected -> StopMicAndSendData()
    //Step 3 OnTextDoneMessage -> SetupText()
    //Step 4 OnJobDone && OnAudioReady -> CalculateFacsStuff()
    //Step 5 PlayAudio()
    //Step 6 OnAudioDone -> StartMicrophone()


    public void StartMicrophone()
    {
        resetButton.interactable = false;
        audioRecorder.StartRecordingMode();

        tvAnimator.Play("Anim_Record");

        if (lookingStateManager.AsleepState.FranticLookAround)
        {
            lookingStateManager.SwitchState(lookingStateManager.ListeningState);
        }
    }

    public void StopMicrophone()
    {
        allowedToSpeak = false;

        StopMicAndSendData();

        if (lookingStateManager.AsleepState.FranticLookAround)
        {
            lookingStateManager.SwitchState(lookingStateManager.ThinkingState);
        }

        tvAnimator.Play("Anim_Thinking");
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
        tvAnimator.Play("Anim_Talking");
    }

    private void LongSentence(float duration)
    {
        audioPlayer.PlayAudio(); // DO OTHER SHIT BEFORE THIS

        //TODO Whatever you want to do with long sentences
        Debug.Log("Long sentence");
        tvAnimator.Play("Anim_Talking");
    }


    private void OnAudioDone()
    {
        StartMicrophone();
        lookingStateManager.SwitchState(lookingStateManager.AttentionState);
        allowedToSpeak = true;
        tvAnimator.Play("Anim_Talk");
        if (!lookingStateManager.AsleepState.StillAsleep)
        {
            resetButton.interactable = true;
        }
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
        resetButton.interactable = false;
        lookingStateManager.SwitchState(lookingStateManager.AsleepState);
    }
}