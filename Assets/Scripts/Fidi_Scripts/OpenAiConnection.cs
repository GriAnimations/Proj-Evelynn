using System;
using System.ClientModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using UnityEngine;

public class OpenAiConnection : MonoBehaviour
{
    private string gptModel = "gpt-4o-mini";

    [TextArea(3, 20)] [SerializeField] private string systemPrompt1 =
        "You are an Assistant that receives parts of a conversation." +
        "You will use the Facial Action Coding System to provide what expressions should be made when the conversation phrases are said." +
        "Always use 3 word phrases. Select a maximum of 2 phrases per sentence when the emotions should be switched." +
        "Return the results as shown in the following Example:\n\n " +
        "{\n" +
        "   \"PhraseFacsPairs\":[\n  " +
        "      {\n" +
        "          \"Phrase\":\"3WordPhrase\",\n" +
        "          \"FacsCodes\": [\"FacsExample1\", \"FacsExample2\", \"FacsExample3\"]\n" +
        "      },\n  " +
        "      {\n" +
        "          \"Phrase\":\"3WordPhrase\",\n" +
        "          \"FacsCodes\": [\"FacsExample1\", \"FacsExample2\"]\n" +
        "      },\n" +
        "      {\n" +
        "          \"Phrase\":\"3WordPhrase\",\n" +
        "          \"FacsCodes\": [\"FacsExample1\", \"FacsExample2\", \"FacsExample3\"]\n" +
        "      }\n" +
        "   ] " +
        "}";

    private Queue<(ChatMessage[], Guid)> jobQueue = new();

    private Coroutine jobQueueCoroutine;
    private Coroutine jobDoneCoroutine;

    public delegate void OnJobDoneDelegate((Guid id, string result) job);

    public event OnJobDoneDelegate OnJobDone;

    public Dictionary<Guid, Task<ClientResult<ChatCompletion>>> tasks = new();

    private List<ChatClientCombo> clientsPool = new();
    private int maxClients = 10;

    public bool Initialize()
    {
        try
        {
            clientsPool.Add(new ChatClientCombo(gptModel));

            jobQueueCoroutine = StartCoroutine(ProcessJobQueue());
            jobDoneCoroutine = StartCoroutine(ProcessJobDone());
        }
        catch (Exception e)
        {
            Debug.LogError("OpenAiClient error: " + e.Message);
            return false;
        }

        Debug.LogWarning("OpenAiClient initialized");
        return true;
    }

    public void Dispose()
    {
        StopJobQueue();
    }

    private IEnumerator ProcessJobDone()
    {
        while (true)
        {
            List<Guid> guids = new List<Guid>();

            foreach (var task in tasks.Where(task => task.Value.IsCompleted))
            {
                OnJobDone?.Invoke((task.Key, task.Value.Result.Value.Content[0].Text));
                Debug.LogWarning("Job done...");
                guids.Add(task.Key);
            }

            foreach (var guid in guids)
            {
                tasks.Remove(guid);
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator ProcessJobQueue()
    {
        while (true)
        {
            if (jobQueue.Count > 0)
            {
                ChatClientCombo client = GetUnusedClient();

                if (client != null)
                {
                    var job = jobQueue.Dequeue();
                    tasks.Add(job.Item2, client.client.CompleteChatAsync(job.Item1));
                }
                else
                {
                    Debug.LogError("No available clients");
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public ChatClientCombo GetUnusedClient()
    {
        ChatClientCombo client = clientsPool.FirstOrDefault(client => !client.inUse);

        if (client != null)
        {
            client.SetInUse(true);
        }
        else if (clientsPool.Count < maxClients)
        {
            client = new ChatClientCombo(gptModel);
            clientsPool.Add(client);
        }
        else
        {
            return null;
        }

        return client;
    }


    public void AddJob(string job, Guid id)
    {
        ChatMessage message = ChatMessage.CreateSystemMessage(systemPrompt1);

        ChatMessage userMessage = ChatMessage.CreateUserMessage(job);

        jobQueue.Enqueue((new[] { message, userMessage }, id));
    }

    public void AddJob(string systemPrompt, string job, Guid id)
    {
        ChatMessage message = ChatMessage.CreateSystemMessage(systemPrompt);

        ChatMessage userMessage = ChatMessage.CreateUserMessage(job);

        jobQueue.Enqueue((new[] { message, userMessage }, id));
    }

    public void StopJobQueue()
    {
        if (jobQueueCoroutine != null)
            StopCoroutine(jobQueueCoroutine);

        if (jobDoneCoroutine != null)
            StopCoroutine(jobDoneCoroutine);
    }
}

public class ChatClientCombo
{
    public ChatClient client;
    public Guid id;
    public bool inUse;

    public ChatClientCombo(ChatClient client, Guid id, bool inUse)
    {
        this.client = client;
        this.id = id;
        this.inUse = inUse;
    }

    public ChatClientCombo(ChatClient client)
    {
        this.client = client;
        this.id = Guid.Empty;
        this.inUse = false;
    }

    public ChatClientCombo(string gptModel)
    {
        client = new OpenAIClient(JsonSecretsReader.GetSecret("apiKey"))
            .GetChatClient(gptModel);
        id = Guid.NewGuid();
        inUse = false;
    }

    public void SetInUse(bool inUse)
    {
        this.inUse = inUse;
    }

    public void GenerateId()
    {
        this.id = Guid.NewGuid();
    }
}