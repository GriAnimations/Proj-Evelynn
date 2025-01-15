using System;
using UnityEngine;

public static class Utility
{
    public static float[] PCMToFloat(byte[] bytes)
    {
        int length = bytes.Length / 2;
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            short sample = (short)(bytes[i * 2] | bytes[i * 2 + 1] << 8);
            samples[i] = sample / (float)short.MaxValue;
        }

        return samples;
    }
    
    public static byte[] FloatToPCM(float[] samples)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * short.MaxValue);
        }

        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        return bytesData;
    }

    
    public static string[] ConvertAudioClipToBase64Chunks(AudioClip audioClip, int chunkSize = 1024)
    {
        // Step 1: Extract raw audio data
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        // Step 2: Convert float array to byte array
        byte[] audioBytes = new byte[samples.Length * 4]; // 4 bytes per float
        Buffer.BlockCopy(samples, 0, audioBytes, 0, audioBytes.Length);

        // Step 3: Encode byte array to Base64
        string base64String = Convert.ToBase64String(audioBytes);

        // Step 4: Split the Base64 string into chunks
        int totalChunks = (base64String.Length + chunkSize - 1) / chunkSize;
        string[] base64Chunks = new string[totalChunks];

        for (int i = 0; i < totalChunks; i++)
        {
            int start = i * chunkSize;
            int length = Math.Min(chunkSize, base64String.Length - start);
            base64Chunks[i] = base64String.Substring(start, length);
        }

        return base64Chunks;
    }
    
    public static string[] ConvertAudioClipToBase64Chunks(float[] samples, int chunkSize = 1024)
    {
        // Step 2: Convert float array to byte array
        byte[] audioBytes = new byte[samples.Length * 4]; // 4 bytes per float
        Buffer.BlockCopy(samples, 0, audioBytes, 0, audioBytes.Length);

        // Step 3: Encode byte array to Base64
        string base64String = Convert.ToBase64String(audioBytes);

        // Step 4: Split the Base64 string into chunks
        int totalChunks = (base64String.Length + chunkSize - 1) / chunkSize;
        string[] base64Chunks = new string[totalChunks];

        for (int i = 0; i < totalChunks; i++)
        {
            int start = i * chunkSize;
            int length = Math.Min(chunkSize, base64String.Length - start);
            base64Chunks[i] = base64String.Substring(start, length);
        }

        return base64Chunks;
    }
    
    public static AudioClip ConvertBase64ChunksToAudioClip(string[] base64Chunks, int sampleRate = 24000)
    {
        // Step 1: Combine Base64 chunks into a single string
        string base64String = string.Join("", base64Chunks);

        // Step 2: Decode Base64 string to byte array
        byte[] audioBytes = Convert.FromBase64String(base64String);

        // Step 3: Convert byte array to float array
        float[] samples = new float[audioBytes.Length / 4]; // 4 bytes per float
        Buffer.BlockCopy(audioBytes, 0, samples, 0, audioBytes.Length);

        // Step 4: Create AudioClip from float array
        AudioClip audioClip = AudioClip.Create("AudioClip", samples.Length, 1, sampleRate, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }



        
}