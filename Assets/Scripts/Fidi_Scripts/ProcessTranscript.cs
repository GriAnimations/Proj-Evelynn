using System.Collections.Generic;

namespace DefaultNamespace
{
    public static class ProcessTranscript 
    {
        private static char[] delimiters = { '.', '?', '!', '\n' };

        public static List<string> SplitTranscript(string transcript)
        {
            List<string> sentences = new List<string>();
            string[] splitTranscript = transcript.Split(delimiters);
            foreach (string sentence in splitTranscript)
            {
                if (sentence.Length > 0)
                {
                    sentences.Add(sentence);
                }
            }
            return sentences;
        }
        
        

        
        
        
    }
}