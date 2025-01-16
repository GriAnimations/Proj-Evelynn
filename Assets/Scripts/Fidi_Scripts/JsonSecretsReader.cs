using System;
using Newtonsoft.Json;
using UnityEngine;


public static class JsonSecretsReader
{
    private static Secrets _secrets;

    public static string GetSecret(string key)
    {
        try
        {
            if (_secrets != null)
            {
                return _secrets.GetSecret(key);
            }

            string json = System.IO.File.ReadAllText("secrets.json");
            _secrets = JsonConvert.DeserializeObject<Secrets>(json);
            return _secrets.GetSecret(key);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading secrets: " + e.Message);
        }
        
        return null;
    }
}

[Serializable]
public class Secrets
{
    public string apiKey;

    public string GetSecret(string key)
    {
        return key switch
        {
            "apiKey" => apiKey,
            _ => null
        };
    }
}