/*This script manages the NPC chat interface, handling message display, input, and communication with the LLM API.*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCChatUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main chat panel GameObject")]
    public GameObject chatPanel;
    
    [Tooltip("Scrollable area for chat messages")]
    public ScrollRect chatScrollRect;
    
    [Tooltip("Content container for messages")]
    public Transform messageContainer;
    
    [Tooltip("Input field for typing messages")]
    public TMP_InputField messageInput;
    
    [Tooltip("Send button")]
    public Button sendButton;
    
    [Tooltip("Close button")]
    public Button closeButton;
    
    [Tooltip("NPC name display")]
    public TMP_Text npcNameText;
    
    [Header("Message Prefabs")]
    [Tooltip("Prefab for user messages (optional - can use instantiated text)")]
    public GameObject userMessagePrefab;
    
    [Tooltip("Prefab for NPC messages (optional - can use instantiated text)")]
    public GameObject npcMessagePrefab;
    
    [Header("Settings")]
    [Tooltip("Typing indicator GameObject (optional)")]
    public GameObject typingIndicator;
    
    [Tooltip("Maximum messages to keep in history")]
    public int maxMessageHistory = 50;
    
    [Header("Player Reference")]
    [Tooltip("Player GameObject (for disabling movement when chat is open)")]
    public GameObject player;
    
    // Current NPC being talked to
    private NPCController currentNPC;
    
    // Store original movement state
    private Movement_Player movementPlayer;
    private bool wasMovementEnabled = true;
    
    // Message history
    private List<GameObject> messageObjects = new List<GameObject>();
    
    // State
    private bool isChatOpen = false;
    
    // References for API (will be added in next step)
    private GeminiAPIClient apiClient;
    
    void Start()
    {
        // Hide chat panel at start
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        
        // Try to find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj;
            }
        }
        
        // Get Movement_Player component (handles both movement and camera rotation)
        if (player != null)
        {
            movementPlayer = player.GetComponent<Movement_Player>();
        }
        
        // Setup button listeners and layout
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessage);
            
            // Fix Send button layout
            RectTransform sendRect = sendButton.GetComponent<RectTransform>();
            if (sendRect != null)
            {
                // Anchor to bottom-right
                sendRect.anchorMin = new Vector2(1f, 0f);
                sendRect.anchorMax = new Vector2(1f, 0f);
                sendRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Position at bottom-right corner
                sendRect.anchoredPosition = new Vector2(-70f, 65f); // 70px from right, 65px from bottom
                sendRect.sizeDelta = new Vector2(120f, 40f); // 120px wide, 40px tall
                
                Debug.Log("NPCChatUI: Send button layout fixed");
            }
            
            Debug.Log("NPCChatUI: Send button found and listener added");
        }
        else
        {
            Debug.LogWarning("NPCChatUI: Send button is NULL!");
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseChat);
            
            // Fix Close button layout
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            if (closeRect != null)
            {
                // Anchor to top-right
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Position at top-right corner
                closeRect.anchoredPosition = new Vector2(-50f, -30f); // 50px from right, 30px from top
                closeRect.sizeDelta = new Vector2(80f, 40f); // 80px wide, 40px tall
                
                Debug.Log("NPCChatUI: Close button layout fixed");
            }
            
            Debug.Log("NPCChatUI: Close button found and listener added");
        }
        else
        {
            Debug.LogError("NPCChatUI: Close button is NULL! You need to assign it in Inspector or create it!");
        }
        
        // Setup input field (Enter key to send)
        if (messageInput != null)
        {
            messageInput.onSubmit.AddListener(delegate { SendMessage(); });
        }
        
        // Hide typing indicator
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }
        
        // Try to find API client (will retry when opening chat if not found)
        FindAPIClient();
    }
    
    void Update()
    {
        // Close chat with ESC key
        if (isChatOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }
        
        // CRITICAL: Force cursor to stay unlocked while chat is open
        // This prevents other scripts from locking it
        if (isChatOpen)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Debug.Log("NPCChatUI: Cursor was locked, forcing unlock again");
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
                Debug.Log("NPCChatUI: Cursor was hidden, making visible again");
            }
        }
    }
    
    /// <summary>
    /// Opens the chat interface for the specified NPC
    /// </summary>
    /// <summary>
    /// Finds the GeminiAPIClient in the scene (retries if not found initially)
    /// </summary>
    private void FindAPIClient()
    {
        if (apiClient == null)
        {
            apiClient = FindObjectOfType<GeminiAPIClient>();
            if (apiClient == null)
            {
                Debug.LogWarning("NPCChatUI: GeminiAPIClient not found. Chat will work but API calls won't function.");
            }
        }
    }
    
    public void OpenChat(NPCController npc)
    {
        Debug.Log("NPCChatUI: OpenChat called");
        
        if (npc == null)
        {
            Debug.LogError("NPCChatUI: Cannot open chat - NPC is null!");
            return;
        }
        
        // Retry finding API client when opening chat (in case it wasn't ready before)
        FindAPIClient();
        
        currentNPC = npc;
        isChatOpen = true;
        
        // Show chat panel
        if (chatPanel != null)
        {
            Debug.Log("NPCChatUI: Activating chat panel - " + chatPanel.name);
            
            // Check parent Canvas
            Canvas parentCanvas = chatPanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log("NPCChatUI: Parent Canvas found - " + parentCanvas.name + ", Active: " + parentCanvas.gameObject.activeSelf);
            }
            else
            {
                Debug.LogWarning("NPCChatUI: No parent Canvas found!");
            }
            
            // Check RectTransform
            RectTransform rectTransform = chatPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"NPCChatUI: Panel position - X:{rectTransform.position.x}, Y:{rectTransform.position.y}, Z:{rectTransform.position.z}");
                Debug.Log($"NPCChatUI: Panel size - Width:{rectTransform.rect.width}, Height:{rectTransform.rect.height}");
                Debug.Log($"NPCChatUI: Panel anchored position - X:{rectTransform.anchoredPosition.x}, Y:{rectTransform.anchoredPosition.y}");
            }
            
            chatPanel.SetActive(true);
            
            // Force panel to be visible and centered (using existing rectTransform variable)
            if (rectTransform != null)
            {
                // CRITICAL: Reset scale to normal (1,1,1) - fixes invisible panel bug
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                Debug.Log("NPCChatUI: Panel scale reset to (1, 1, 1)");
                
                // Set anchors to center (not stretch!) - this is crucial
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                // Reset Z position to 0 (for Screen Space Overlay)
                Vector3 currentPos = rectTransform.anchoredPosition3D;
                rectTransform.anchoredPosition3D = new Vector3(0f, 0f, 0f);
                
                // Set to center of screen
                rectTransform.anchoredPosition = Vector2.zero;
                
                // Set reasonable size for chat panel (800x600 for better visibility)
                rectTransform.sizeDelta = new Vector2(800, 600);
                Debug.Log("NPCChatUI: Panel configured - Anchors: center, Position: (0,0,0), Size: 800x600, Scale: (1,1,1)");
            }
            
            Debug.Log("NPCChatUI: Chat panel active state: " + chatPanel.activeSelf);
            Debug.Log("NPCChatUI: Chat panel active in hierarchy: " + chatPanel.activeInHierarchy);
        }
        else
        {
            Debug.LogError("NPCChatUI: Chat Panel is NULL! Cannot show chat UI!");
            return;
        }
        
        // Set NPC name and fix layout
        if (npcNameText != null)
        {
            npcNameText.text = npc.GetNPCName();
            
            // Fix NPC name text layout
            RectTransform nameRect = npcNameText.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                // Anchor to top-left/center
                nameRect.anchorMin = new Vector2(0f, 1f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Position at top center
                nameRect.anchoredPosition = new Vector2(0f, -30f); // Centered, 30px from top
                nameRect.sizeDelta = new Vector2(-100f, 40f); // Full width minus margins, 40px tall
                
                Debug.Log("NPCChatUI: NPC name layout fixed");
            }
            
            Debug.Log("NPCChatUI: NPC name set to - " + npc.GetNPCName());
        }
        else
        {
            Debug.LogWarning("NPCChatUI: NPC Name Text is null!");
        }
        
        // Fix ChatScrollView layout
        if (chatScrollRect != null)
        {
            RectTransform scrollRect = chatScrollRect.GetComponent<RectTransform>();
            if (scrollRect != null)
            {
                // Stretch to fill middle area (between name and input)
                scrollRect.anchorMin = new Vector2(0f, 0f);
                scrollRect.anchorMax = new Vector2(1f, 1f);
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Margins: 20px left/right, 70px top (below name), 120px bottom (above input)
                scrollRect.offsetMin = new Vector2(20f, 120f); // Left, Bottom
                scrollRect.offsetMax = new Vector2(-20f, -70f); // Right, Top
                
                Debug.Log("NPCChatUI: ChatScrollView layout fixed");
            }
        }
        
        // Fix and focus input field
        if (messageInput != null)
        {
            // Fix MessageInput layout (ensure it's properly sized)
            RectTransform inputRect = messageInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                // Set anchors to bottom-stretch (left to right)
                inputRect.anchorMin = new Vector2(0f, 0f);
                inputRect.anchorMax = new Vector2(1f, 0f);
                inputRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Position at bottom with proper margins
                inputRect.anchoredPosition = new Vector2(0f, 65f); // Centered horizontally, 65px from bottom
                inputRect.offsetMin = new Vector2(20f, inputRect.offsetMin.y); // 20px left margin
                inputRect.offsetMax = new Vector2(-150f, inputRect.offsetMax.y); // 150px right margin (for Send button)
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, 40f); // 40px height
                
                Debug.Log("NPCChatUI: MessageInput layout fixed");
            }
            
            messageInput.text = "";
            messageInput.Select();
            messageInput.ActivateInputField();
            Debug.Log("NPCChatUI: Input field activated");
        }
        else
        {
            Debug.LogWarning("NPCChatUI: Message Input is null!");
        }
        
        // Disable player movement and camera rotation (both handled by Movement_Player)
        if (movementPlayer != null)
        {
            wasMovementEnabled = movementPlayer.enabled;
            movementPlayer.enabled = false;
            Debug.Log("NPCChatUI: Player movement and camera rotation disabled");
        }
        
        // Unlock cursor for UI interaction - use coroutine to ensure it sticks
        StartCoroutine(ForceUnlockCursor());
        
        // Add welcome message
        AddNPCMessage("Hello! How can I help you today?");
        Debug.Log("NPCChatUI: Welcome message added");
    }
    
    /// <summary>
    /// Closes the chat interface
    /// </summary>
    public void CloseChat()
    {
        if (!isChatOpen)
        {
            return;
        }
        
        Debug.Log("NPCChatUI: CloseChat called");
        
        isChatOpen = false;
        
        // Store NPC reference before clearing
        NPCController npcToNotify = currentNPC;
        currentNPC = null;
        
        // Hide chat panel
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        
        // Hide typing indicator
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }
        
        // Re-enable player movement and camera rotation
        if (movementPlayer != null && wasMovementEnabled)
        {
            movementPlayer.enabled = true;
            Debug.Log("NPCChatUI: Player movement and camera rotation re-enabled");
        }
        
        // Lock cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("NPCChatUI: Cursor locked for gameplay");
        
        // CRITICAL: Deselect any UI element so game keys work again
        // The EventSystem check in WaypointSystem blocks input when something is selected
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("NPCChatUI: Deselected UI element for game input");
        }
        
        // Notify NPC that interaction ended
        if (npcToNotify != null)
        {
            // Call EndInteraction which will reset the NPC state and notify the detector
            npcToNotify.EndInteraction();
            Debug.Log("NPCChatUI: Notified NPC that interaction ended");
        }
        
        // FAILSAFE: Directly reset the NPCInteractionDetector state
        // Use FindObjectOfType to ensure we always find it, regardless of player tagging
        NPCInteractionDetector detector = FindObjectOfType<NPCInteractionDetector>();
        if (detector != null)
        {
            detector.OnInteractionEnd();
            Debug.Log("NPCChatUI: Directly reset NPCInteractionDetector state (failsafe)");
        }
        else
        {
            Debug.LogWarning("NPCChatUI: Could not find NPCInteractionDetector to reset!");
        }
    }
    
    /// <summary>
    /// Sends the current message from input field
    /// </summary>
    public void SendMessage()
    {
        if (messageInput == null || string.IsNullOrWhiteSpace(messageInput.text))
        {
            return;
        }
        
        string userMessage = messageInput.text.Trim();
        
        // Clear input field
        messageInput.text = "";
        messageInput.Select();
        messageInput.ActivateInputField();
        
        // Add user message to chat
        AddUserMessage(userMessage);
        
        // Show typing indicator
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(true);
        }
        
        // Send to API and get response
        StartCoroutine(GetAIResponse(userMessage));
    }
    
    /// <summary>
    /// Gets AI response from API (coroutine for async)
    /// </summary>
    private IEnumerator GetAIResponse(string userMessage)
    {
        if (apiClient != null)
        {
            // Use API client to get response
            yield return StartCoroutine(apiClient.SendMessage(userMessage, OnAPIResponse));
        }
        else
        {
            // Fallback: Show placeholder message
            yield return new WaitForSeconds(1f); // Simulate thinking
            AddNPCMessage("I'm still learning! API integration coming soon. You asked: \"" + userMessage + "\"");
            
            if (typingIndicator != null)
            {
                typingIndicator.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Callback for API response
    /// </summary>
    private void OnAPIResponse(string response, bool success)
    {
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }
        
        if (success && !string.IsNullOrEmpty(response))
        {
            AddNPCMessage(response);
        }
        else
        {
            AddNPCMessage("Sorry, I'm having trouble connecting right now. Please try again later.");
        }
    }
    
    /// <summary>
    /// Adds a user message to the chat
    /// </summary>
    void AddUserMessage(string message)
    {
        CreateMessage(message, true);
    }
    
    /// <summary>
    /// Adds an NPC message to the chat
    /// </summary>
    void AddNPCMessage(string message)
    {
        CreateMessage(message, false);
    }
    
    /// <summary>
    /// Creates a message UI element
    /// </summary>
    void CreateMessage(string message, bool isUser)
    {
        if (messageContainer == null)
        {
            Debug.LogError("NPCChatUI: Message container is null!");
            return;
        }
        
        GameObject messageObj;
        
        // Use prefab if available, otherwise create simple text
        if (isUser && userMessagePrefab != null)
        {
            messageObj = Instantiate(userMessagePrefab, messageContainer);
        }
        else if (!isUser && npcMessagePrefab != null)
        {
            messageObj = Instantiate(npcMessagePrefab, messageContainer);
        }
        else
        {
            // Create simple text message
            messageObj = new GameObject(isUser ? "UserMessage" : "NPCMessage");
            messageObj.transform.SetParent(messageContainer);
            
            TMP_Text textComponent = messageObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = message;
            textComponent.fontSize = 16;
            textComponent.color = isUser ? Color.white : Color.cyan;
            textComponent.alignment = isUser ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            
            // Add layout component for proper sizing
            ContentSizeFitter fitter = messageObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        // Set text if using prefab
        TMP_Text text = messageObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = message;
        }
        
        // Add to history
        messageObjects.Add(messageObj);
        
        // Limit history size
        if (messageObjects.Count > maxMessageHistory)
        {
            GameObject oldMessage = messageObjects[0];
            messageObjects.RemoveAt(0);
            Destroy(oldMessage);
        }
        
        // Scroll to bottom
        StartCoroutine(ScrollToBottom());
    }
    
    /// <summary>
    /// Forces cursor unlock with delay to ensure it overrides any other scripts
    /// </summary>
    IEnumerator ForceUnlockCursor()
    {
        // Unlock immediately
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("NPCChatUI: Cursor unlocked (immediate)");
        
        // Wait a frame and unlock again (to override any other scripts)
        yield return new WaitForEndOfFrame();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("NPCChatUI: Cursor unlocked (frame delayed)");
        
        // Wait another small delay and unlock once more
        yield return new WaitForSeconds(0.1f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("NPCChatUI: Cursor unlocked (0.1s delayed - final)");
    }
    
    /// <summary>
    /// Scrolls chat to bottom (with small delay for layout update)
    /// </summary>
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    /// <summary>
    /// Check if chat is currently open
    /// </summary>
    public bool IsChatOpen()
    {
        return isChatOpen;
    }
}

