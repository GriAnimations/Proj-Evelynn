using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Fidi_Scripts
{
    public class ConversationLogger : MonoBehaviour
    {
        public int messageIDCounter = 1;

        private string conversationFolderPath;
        private string conversationLogFilePath;
        
        public bool logging = false;
        public bool started = false;
        
        
        public void StartConversation()
        {
            if (started)
            {
                Debug.LogWarning("Conversation already started!");
                return;
            }
            
            started = true;
            // Example path structure:
            //   Application.persistentDataPath/Conversations/Conversation_20250116_153045
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderName = $"Conversation_{timestamp}";

            // Combine paths so they work across different platforms
            conversationFolderPath = Path.Combine(Application.dataPath, "Conversations", folderName);
        
            // Ensure the directory is created
            Directory.CreateDirectory(conversationFolderPath);

            // Create a text file for logging conversation
            conversationLogFilePath = Path.Combine(conversationFolderPath, "conversation.txt");
        
            // Initialize log file with a header
            File.WriteAllText(
                conversationLogFilePath,
                $"Conversation started at {DateTime.Now}\n\n"
            );

            // Reset message ID counter (optional, depending on your desired behavior)
            messageIDCounter = 1;
        
            Debug.Log($"New conversation folder created: {conversationFolderPath}");
        }

        public void EndConversation()
        {
            if (!started)
            {
                Debug.LogWarning("No conversation started! Call StartConversation() first.");
                return;
            }
            
            started = false;
            Debug.Log("Conversation ended.");
        }
        
        
        /// <summary>
        /// Add a message to the conversation log. Optionally pass in an additional text.
        /// Also, save a WAV file named using the ID and Username (stubbed out in this example).
        /// </summary>
        /// <param name="user">The username for the message.</param>
        /// <param name="message">The content of the message.</param>
        /// <param name="optionalText">An optional extra field.</param>
        public int AddMessage(string user, string message, string optionalText = "")
        {
            if (string.IsNullOrEmpty(conversationFolderPath))
            {
                Debug.LogWarning("No conversation started! Call StartConversation() first.");
                return -1;
            }

            // Get next message ID
            int currentID = messageIDCounter++;

            // Prepare log text
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Date: {DateTime.Now}");
            sb.AppendLine($"ID: {currentID}");
            sb.AppendLine($"Username: {user}");
            sb.AppendLine($"Message: {message}");
        
            // Only log optional text if not empty
            if (!string.IsNullOrEmpty(optionalText))
            {
                sb.AppendLine($"Optional: {optionalText}");
            }

            sb.AppendLine();  // Blank line for readability

            // Append to our conversation log file
            File.AppendAllText(conversationLogFilePath, sb.ToString());

            Debug.Log($"Message with ID {currentID} by '{user}' added to conversation.");
            return currentID;
        }

        public void SaveWavFile(AudioClip clip, int messageId, string user)
        {
            // Create a subfolder for audio inside the conversation folder
            string audioFolderPath = Path.Combine(conversationFolderPath, "Audio");
            Directory.CreateDirectory(audioFolderPath);
            
            string filename = $"{messageId.ToString("D3")}_{user}.wav";
            string filePath = Path.Combine(audioFolderPath, filename);
            
            SaveWav.Save(filePath, clip);
        }
    }
}
