/*This script handles communication with the Python backend server for LLM responses.
 * The server runs FastAPI + LangChain + ChromaDB + Gemini for RAG-based answers.*/
using UnityEngine;
using System.Collections;
using System.Text;

public class GeminiAPIClient : MonoBehaviour
{
    [Header("Server Configuration")]
    [Tooltip("URL of the Python backend server")]
    public string serverUrl = "http://localhost:8000";
    
    [Tooltip("API endpoint path")]
    public string chatEndpoint = "/api/chat";
    
    [Header("Settings")]
    [Tooltip("Request timeout in seconds")]
    public float requestTimeout = 30f;
    
    // Callback type for API responses
    public delegate void APIResponseCallback(string response, bool success);
    
    void Start()
    {
        Debug.Log($"GeminiAPIClient: Configured to use server at {serverUrl}{chatEndpoint}");
    }
    
    /// <summary>
    /// Sends a message to the Python backend and gets a RAG-powered response
    /// </summary>
    public IEnumerator SendMessage(string userMessage, APIResponseCallback callback)
    {
        string url = $"{serverUrl}{chatEndpoint}";
        
        // Create request body JSON
        string jsonBody = "{\"text\":\"" + EscapeJsonString(userMessage) + "\"}";
        byte[] bodyData = Encoding.UTF8.GetBytes(jsonBody);
        
        Debug.Log($"GeminiAPIClient: Sending request to {url}");
        
        // Send request
        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
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
                    callback?.Invoke("Sorry, the request timed out. Please try again.", false);
                    yield break;
                }
                yield return null;
            }
            
            // Process response
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"GeminiAPIClient: Received response: {responseText}");
                
                string extractedResponse = ParseResponse(responseText);
                callback?.Invoke(extractedResponse, true);
            }
            else
            {
                Debug.LogError($"GeminiAPIClient: Request failed: {request.error}");
                Debug.LogError($"GeminiAPIClient: Response code: {request.responseCode}");
                
                // Provide helpful error message based on error type
                string errorMessage = "Sorry, I couldn't connect to my brain. ";
                if (request.responseCode == 0)
                {
                    errorMessage += "The server might not be running. Please start the Python server.";
                }
                else
                {
                    errorMessage += "Please try again later.";
                }
                
                callback?.Invoke(errorMessage, false);
            }
        }
    }
    
    /// <summary>
    /// Escapes special characters for JSON string
    /// </summary>
    string EscapeJsonString(string str)
    {
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
    
    /// <summary>
    /// Parses the API response JSON to extract the text
    /// </summary>
    string ParseResponse(string jsonResponse)
    {
        try
        {
            // Response format: {"response": "The answer here"}
            int responseStart = jsonResponse.IndexOf("\"response\":\"");
            if (responseStart == -1)
            {
                // Try alternate format with space after colon
                responseStart = jsonResponse.IndexOf("\"response\": \"");
                if (responseStart == -1)
                {
                    Debug.LogWarning("GeminiAPIClient: Could not find 'response' field in JSON");
                    return "Sorry, I received an unexpected response format.";
                }
                responseStart += 13; // Skip past "response": "
            }
            else
            {
                responseStart += 12; // Skip past "response":"
            }
            
            // Find the end of the response string
            int responseEnd = -1;
            bool escaped = false;
            for (int i = responseStart; i < jsonResponse.Length; i++)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }
                if (jsonResponse[i] == '\\')
                {
                    escaped = true;
                    continue;
                }
                if (jsonResponse[i] == '"')
                {
                    responseEnd = i;
                    break;
                }
            }
            
            if (responseEnd == -1)
            {
                Debug.LogWarning("GeminiAPIClient: Could not find end of response string");
                return "Sorry, I couldn't process that response.";
            }
            
            string text = jsonResponse.Substring(responseStart, responseEnd - responseStart);
            
            // Unescape JSON string
            text = text
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
            
            return text;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GeminiAPIClient: Error parsing response: {e.Message}");
            return "Sorry, there was an error processing the response.";
        }
    }
}
