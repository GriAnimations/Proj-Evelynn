using System;
using Newtonsoft.Json;
using UnityEngine;

public static class JasonDecoder
{
    public static JsonReturn DecodeJason(string jason)
    {
        //JsonReturn jsonFile = new JsonReturn(new[]
        //{
        //    new PhraseFacsPair("Phrase 1", new[] {"Facs 1", "Facs 2", "Facs 3"}),
        //    new PhraseFacsPair("Phrase 2", new[] {"Facs 1", "Facs 2", "Facs 3"}),
        //    new PhraseFacsPair("Phrase 3", new[] {"Facs 1", "Facs 2", "Facs 3"})
        //});

        return JsonConvert.DeserializeObject<JsonReturn>(jason);
        
        //Debug.Log(JsonUtility.ToJson(jsonFile));
        
        //return "";
    }
}

/*
 [
  {
    "Phrase":"3WordPhrase",
    "FacsCodes": ["FacsExample1", "FacsExample2", "FacsExample3"]
  },
  {
    "Phrase":"3WordPhrase",
    "FacsCodes": ["FacsExample1", "FacsExample2", "FacsExample3"]
  },
  {
    "Phrase":"3WordPhrase",
    "FacsCodes": ["FacsExample1", "FacsExample2", "FacsExample3"]
  }
] 
 */

[Serializable]
public class JsonReturn
{
    public PhraseFacsPair[] PhraseFacsPairs;

    public JsonReturn(PhraseFacsPair[] sentences)
    {
        PhraseFacsPairs = sentences;
    }
}

[Serializable]

public class PhraseFacsPair
{
    public string Phrase;
    public string[] FacsCodes;

    public PhraseFacsPair(string phrase, string facsCode)
    {
        Phrase = phrase;
        FacsCodes = new[] {facsCode};
    }
    [JsonConstructor]
    public PhraseFacsPair(string phrase, string[] facsCodes)
    {
        Phrase = phrase;
        FacsCodes = facsCodes;
    }
}