# NPC System with LLM Integration - Complete Implementation Plan

##  Overview

This document outlines the complete plan for adding an interactive NPC system to the Unity 3D University building project. The NPC will:
1. Have animations (idle, talking, greeting)
2. Be interactable when the player is nearby (press E)
3. Open a chat interface for communication
4. Use Gemini LLM API (via Gemini Studio) to answer questions
5. Use vectorized storage to provide accurate university information

---

##  System Architecture

### High-Level Flow
```
Player Approaches NPC → Detection System → Show "Press E" Prompt → 
Player Presses E → Open Chat UI → Player Types Question → 
Send to Gemini API with Context → Receive Response → Display in Chat
```

---

##  File Structure & Implementation Locations

### **NEW FILES TO CREATE:**

#### 1. **Scripts Folder** (`Unity Project/Building Project P.3/Assets/Scripts/`)

**a. `NPCController.cs`** - Main NPC behavior controller
- Handles NPC animations (idle, talking, greeting)
- Manages NPC state (idle, interacting, talking)
- Detects player proximity
- Triggers interaction when player presses E

**b. `NPCInteractionDetector.cs`** - Player-side interaction detection
- Raycast-based detection (similar to ItemCollector pattern)
- Shows "Press E to Talk" UI when near NPC
- Handles E key press detection
- Communicates with NPCController

**c. `NPCChatUI.cs`** - Chat interface manager
- Manages chat UI panel visibility
- Handles message input/output
- Scrollable chat history
- Send button functionality
- Close chat button

**d. `GeminiAPIClient.cs`** - LLM API communication
- Handles HTTP requests to Gemini Studio API
- Manages API key (stored securely)
- Formats prompts with context
- Processes responses
- Error handling

**e. `VectorStoreManager.cs`** - Vector storage handler
- Manages vectorized university data
- Performs similarity search
- Retrieves relevant context for queries
- Integrates with GeminiAPIClient

**f. `NPCData.cs`** - ScriptableObject for NPC configuration
- NPC name, position, personality
- Conversation context
- Animation references

#### 2. **UI Prefabs** (Create in Unity Editor)
- `NPCChatPanel.prefab` - Main chat interface
- `NPCPromptUI.prefab` - "Press E to Talk" floating text

#### 3. **NPC Assets** (You'll need to add)
- NPC 3D model (character with rigging)
- Animation clips (idle, talking, greeting)
- Animator Controller for NPC

---

##  MODIFICATIONS TO EXISTING FILES

### **1. `Movement_Player.cs`** 
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/Movement_Player.cs`

**Changes:**
- **NO DIRECT MODIFICATIONS NEEDED** - The interaction will be handled by a separate system that doesn't interfere with movement
- The existing `E` key handling in `WaypointSystem.cs` uses a flag system (`EbeingAbledToUse`), so we'll use a similar pattern

### **2. `WaypointSystem.cs`**
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/WaypointSystem.cs`

**Changes:**
- Add a new flag: `public bool NPCInteractionEnabled = true;`
- Modify `HandleInput()` method around line 332 to check for NPC interaction BEFORE checking menu toggle
- Add check: If NPC interaction is active, don't toggle menu

**Specific Code Addition:**
```csharp
// In HandleInput() method, before the existing E key check:
if (Input.GetKeyDown(KeyCode.E) && NPCInteractionEnabled)
{
    // Let NPCInteractionDetector handle it first
    // If no NPC interaction, fall through to menu toggle
}
```

### **3. `ItemCollector.cs`**
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/ItemCollector.cs`

**Changes:**
- **MINIMAL CHANGE** - Add a check to disable NPC interaction UI when ItemCollector is active
- Add reference to NPCInteractionDetector to disable it when collecting items

---

##  Implementation Details

### **Phase 1: NPC Setup & Detection**

#### **NPCController.cs** Structure:
```csharp
- public float interactionRange = 3f
- public Animator npcAnimator
- public GameObject player
- private bool isInteracting = false
- private bool playerInRange = false

Methods:
- Update() - Check player distance
- OnPlayerEnterRange() - Start greeting animation
- OnPlayerExitRange() - Return to idle
- StartInteraction() - Begin chat
- EndInteraction() - Close chat, return to idle
```

#### **NPCInteractionDetector.cs** Structure:
```csharp
- public Camera playerCamera
- public LayerMask npcLayer
- public GameObject interactionPromptUI
- public float detectionRange = 5f
- private NPCController currentNPC = null

Methods:
- Update() - Raycast detection (similar to ItemCollector pattern)
- ShowInteractionPrompt() - Display "Press E to Talk"
- HideInteractionPrompt() - Hide prompt
- OnInteract() - Trigger NPC interaction
```

**Integration Point:**
- Uses the same raycast pattern as `ItemCollector.cs` (lines 74-76)
- Checks for NPC layer instead of item layer
- Shows UI prompt similar to `interactionUI` in ItemCollector

---

### **Phase 2: Chat UI System**

#### **NPCChatUI.cs** Structure:
```csharp
- public GameObject chatPanel
- public TMP_InputField messageInput
- public TMP_Text chatHistory
- public Button sendButton
- public Button closeButton
- public ScrollRect chatScrollRect
- private List<ChatMessage> messageHistory

Methods:
- OpenChat() - Show chat panel, disable player movement
- CloseChat() - Hide panel, re-enable movement
- SendMessage() - Process user input, send to API
- DisplayMessage() - Add message to chat history
- ScrollToBottom() - Auto-scroll to latest message
```

**UI Setup:**
- Create Canvas child for NPC Chat (separate from main UI)
- Chat panel with:
  - Scrollable text area (TMP_Text with ScrollRect)
  - Input field at bottom
  - Send button
  - Close button (X or ESC key)
  - NPC name header

---

### **Phase 3: LLM Integration**

#### **GeminiAPIClient.cs** Structure:
```csharp
- private string apiKey (from environment or config)
- private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"
- private string contextData (from VectorStoreManager)

Methods:
- SendMessage(string userMessage, string[] context) - Main API call
- FormatPrompt() - Combine user message with context
- ParseResponse() - Extract text from API response
- HandleError() - Error handling and fallback messages
```

**API Integration:**
- Use Unity's `UnityWebRequest` for HTTP calls
- Implement async/await pattern or coroutines
- Store API key in a config file (not in code)
- Handle rate limiting and errors gracefully

#### **VectorStoreManager.cs** Structure:
```csharp
- private List<UniversityData> vectorizedData
- private Dictionary<string, float[]> embeddings

Methods:
- LoadUniversityData() - Load from CSV/JSON files
- GenerateEmbeddings() - Create vector embeddings (or use pre-computed)
- SearchSimilar(string query, int topK) - Find relevant context
- GetContextForQuery(string query) - Return formatted context
```

**Data Sources:**
- Use existing CSV files in `Excel Files/` folder:
  - `TeachersInfo.csv` - Teacher information
  - `WinterSemester.csv` / `SpringSemester.csv` - Course schedules
  - `SubjectLists.csv` - Course information
  - `StandardCurriculum.csv` - Curriculum data
  - `DoorNames.csv` - Room/location information

**Vector Storage Options:**
1. **Simple Approach:** Pre-compute embeddings using a local model or API
2. **Advanced Approach:** Use a vector database (Pinecone, Weaviate, or local FAISS)
3. **Hybrid Approach:** Use keyword matching + semantic search

---

### **Phase 4: Integration & Polish**

#### **Scene Setup:**
1. Place NPC GameObject in scene
2. Add Animator component with animations
3. Set up NPC layer in Layer settings
4. Configure NPCController with references
5. Set up chat UI in Canvas hierarchy

#### **Input System:**
- The existing `InputSystem_Actions.inputactions` already has "Interact" action bound to E key
- We can use Unity's new Input System OR continue with `Input.GetKeyDown(KeyCode.E)`
- Recommendation: Use existing pattern for consistency

---

##  Security & Configuration

### **API Key Management:**
Create `Config/NPCConfig.cs` (ScriptableObject):
```csharp
- public string geminiAPIKey
- public string apiEndpoint
- public int maxContextLength
- public float similarityThreshold
```

**Store in:** `Unity Project/Building Project P.3/Assets/Settings/NPCConfig.asset`
- Add to `.gitignore` to prevent committing API keys

---

##  Data Flow Diagram

```
[Player] 
    ↓
[NPCInteractionDetector] (Raycast detection)
    ↓ (Player presses E)
[NPCController] (Start interaction)
    ↓
[NPCChatUI] (Open chat panel)
    ↓ (User types message)
[FastApiServer]
    ↓
[VectorStoreManager] (Search for relevant context)
    ↓
[GeminiAPIClient] (Send query + context to API)
    ↓
[Gemini API] (Process and respond)
    ↓
[NPCChatUI] (Display response)
```

---

##  UI/UX Considerations

1. **Interaction Prompt:**
   - Floating text above NPC: "Press E to Talk"
   - Similar style to existing `interactionUI` in ItemCollector
   - Appears when within 3-5 meters

2. **Chat Interface:**
   - Modern, clean design matching existing UI style
   - Dark/light theme consistent with game
   - Typing indicator when waiting for response
   - Message bubbles (user left, NPC right)
   - Smooth animations for opening/closing

3. **Mobile Support:**
   - Touch-friendly buttons
   - Virtual keyboard support
   - Responsive layout

---

##  Testing Checklist

- [ ] NPC appears in scene with animations
- [ ] Player detection works at correct range
- [ ] "Press E" prompt appears/disappears correctly
- [ ] Chat UI opens/closes smoothly
- [ ] Messages send and receive correctly
- [ ] API calls work with valid API key
- [ ] Vector search returns relevant context
- [ ] Error handling works (no API key, network issues)
- [ ] Mobile controls work
- [ ] No conflicts with existing E key functionality
- [ ] Performance is acceptable (no lag)

---

##  Implementation Order

1. **Week 1: Foundation**
   - Create NPC GameObject and basic setup
   - Implement NPCController and NPCInteractionDetector
   - Set up basic animations
   - Test detection and interaction

2. **Week 2: UI & Chat**
   - Design and implement chat UI
   - Create NPCChatUI script
   - Integrate with NPCController
   - Test UI flow

3. **Week 3: LLM Integration**
   - Set up Gemini API client
   - Implement VectorStoreManager
   - Load and process university data
   - Test API calls and responses

4. **Week 4: Polish & Integration**
   - Fine-tune prompts and context
   - Optimize performance
   - Add error handling
   - Test with real users
   - Documentation

---

##  Key Integration Points

### **Minimal Invasiveness Strategy:**

1. **Separate Layer System:**
   - Create new "NPC" layer
   - Doesn't interfere with existing layers (itemLayer, itemLayer2, etc.)

2. **Flag-Based Interaction:**
   - Use `NPCInteractionEnabled` flag in WaypointSystem
   - Prevents E key conflicts
   - Can be toggled on/off

3. **Independent Scripts:**
   - All NPC functionality in separate scripts
   - No modifications to core movement/waypoint logic
   - Easy to disable/remove if needed

4. **UI Separation:**
   - Chat UI in separate Canvas or Canvas child
   - Doesn't interfere with existing UI elements

---

