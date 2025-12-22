/*This script handles communication with the Gemini API for LLM responses.*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GeminiAPIClient : MonoBehaviour
{
    [Header("API Configuration")]
    [Tooltip("Gemini API Key (keep secret!)")]
    public string apiKey = "";
    
    [Tooltip("API endpoint URL")]
    public string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
    
    [Header("Settings")]
    [Tooltip("Maximum context length")]
    public int maxContextLength = 2000;
    
    [Tooltip("Request timeout in seconds")]
    public float requestTimeout = 30f;
    
    // Reference to vector store manager (for context)
    private VectorStoreManager vectorStore;
    
    // Callback type for API responses
    public delegate void APIResponseCallback(string response, bool success);
    
    void Start()
    {
        // Try to find vector store manager
        vectorStore = FindObjectOfType<VectorStoreManager>();
        if (vectorStore == null)
        {
            Debug.LogWarning("GeminiAPIClient: VectorStoreManager not found. Responses will be without context.");
        }
        
        // Load API key from config if available
        LoadAPIKey();
    }
    
    /// <summary>
    /// Loads API key from configuration (if available)
    /// </summary>
    void LoadAPIKey()
    {
        // Try to load from NPCConfig if it exists
        // For now, API key should be set in inspector or via environment variable
        if (string.IsNullOrEmpty(apiKey))
        {
            // Try environment variable as fallback
            apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        }
    }
    
    /// <summary>
    /// Sends a message to the Gemini API and gets a response
    /// </summary>
    public IEnumerator SendMessage(string userMessage, APIResponseCallback callback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("GeminiAPIClient: API key is not set!");
            callback?.Invoke("", false);
            yield break;
        }
        
        // Get relevant context from vector store
        string[] context = null;
        if (vectorStore != null)
        {
            context = vectorStore.GetContextForQuery(userMessage);
        }
        
        // Format the prompt
        string prompt = FormatPrompt(userMessage, context);
        
        // Create request
        string url = $"{apiEndpoint}?key={apiKey}";
        
        // Create request body (simple JSON format)
        string jsonBody = "{\"contents\":[{\"parts\":[{\"text\":\"" + EscapeJsonString(prompt) + "\"}]}]}";
        byte[] bodyData = Encoding.UTF8.GetBytes(jsonBody);
        
        // Send request
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.PostWwwForm(url, ""))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyData);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            float startTime = Time.time;
            var operation = request.SendWebRequest();
            
            // Wait for response with timeout
            while (!operation.isDone)
            {
                if (Time.time - startTime > requestTimeout)
                {
                    Debug.LogError("GeminiAPIClient: Request timeout!");
                    callback?.Invoke("", false);
                    yield break;
                }
                yield return null;
            }
            
            // Process response
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                string extractedResponse = ParseResponse(responseText);
                callback?.Invoke(extractedResponse, true);
            }
            else
            {
                Debug.LogError($"GeminiAPIClient: API request failed: {request.error}");
                callback?.Invoke("", false);
            }
        }
    }
    
    /// <summary>
    /// Formats the prompt with user message and context
    /// </summary>
    string FormatPrompt(string userMessage, string[] context)
    {
        StringBuilder prompt = new StringBuilder();
        
        // Add system context
        prompt.AppendLine("You are a helpful assistant for the University of Crete Computer Science Department.");
        prompt.AppendLine("Answer questions about the university, courses, teachers, facilities, and general information.");
        prompt.AppendLine("Be friendly, concise, and accurate.");
        prompt.AppendLine();
        
        // Add relevant context if available
        if (context != null && context.Length > 0)
        {
            prompt.AppendLine("Relevant information:");
            foreach (string ctx in context)
            {
                if (!string.IsNullOrEmpty(ctx))
                {
                    prompt.AppendLine($"- {ctx}");
                }
            }
            prompt.AppendLine();
        }
        
        // Add user message
        prompt.AppendLine($"User question: {userMessage}");
        prompt.AppendLine();
        prompt.AppendLine("Please provide a helpful answer:");
        
        return prompt.ToString();
    }
    
    /// <summary>
    /// Escapes JSON string
    /// </summary>
    string EscapeJsonString(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
    
    /// <summary>
    /// Parses the API response JSON to extract the text
    /// </summary>
    string ParseResponse(string jsonResponse)
    {
        try
        {
            // Simple JSON parsing (extract text from response)
            // Gemini response format: {"candidates":[{"content":{"parts":[{"text":"..."}]}}]}
            int textStart = jsonResponse.IndexOf("\"text\":\"");
            if (textStart == -1) return "Sorry, I couldn't process that response.";
            
            textStart += 8; // Skip past "text":"
            int textEnd = jsonResponse.IndexOf("\"", textStart);
            if (textEnd == -1) return "Sorry, I couldn't process that response.";
            
            string text = jsonResponse.Substring(textStart, textEnd - textStart);
            text = text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
            
            return text;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GeminiAPIClient: Error parsing response: {e.Message}");
            return "Sorry, there was an error processing the response.";
        }
    }
    
}

