# 🔌 Code Integration Guide - Exact Modification Points

## 📍 EXACT CODE CHANGES NEEDED

### **1. WaypointSystem.cs - HandleInput() Method**

**File:** `Unity Project/Building Project P.3/Assets/Scripts/WaypointSystem.cs`

**Current Code (around line 326-339):**
```csharp
private void HandleInput()
{
    if (mobileusage == false)
    {
        //If a user presses the letter 'E' on their keyboard...
        if (Input.GetKeyDown(KeyCode.E) && (EbeingAbledToUse == true))
        {
            Game.GetComponent<ItemCollector>().enabled = false;
            ToggleNavigationMode();
        }
        // ... rest of code
    }
}
```

**Modified Code:**
```csharp
// ADD at top of class (around line 42):
public bool NPCInteractionEnabled = true;
public NPCInteractionDetector npcDetector; // Optional reference

// MODIFY HandleInput() method (around line 332):
private void HandleInput()
{
    if (mobileusage == false)
    {
        //If a user presses the letter 'E' on their keyboard...
        if (Input.GetKeyDown(KeyCode.E))
        {
            // NEW: Check for NPC interaction first (takes priority)
            if (NPCInteractionEnabled && npcDetector != null && npcDetector.CanInteract())
            {
                // NPC interaction is active, let NPCInteractionDetector handle it
                npcDetector.OnInteract();
                return; // Don't proceed to menu toggle
            }
            
            // Original menu toggle code (only if no NPC interaction)
            if (EbeingAbledToUse == true)
            {
                Game.GetComponent<ItemCollector>().enabled = false;
                ToggleNavigationMode();
            }
        }
        // ... rest of code remains the same
    }
}
```

---

### **2. ItemCollector.cs - Update() Method (Optional Coordination)**

**File:** `Unity Project/Building Project P.3/Assets/Scripts/ItemCollector.cs`

**Current Code (around line 71-94):**
```csharp
void Update()
{
    Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, 3f, itemLayer))
    {
        currentItem = hit.collider.gameObject;
        interactionUI.SetActive(true);
        // ... rest of code
    }
    // ... rest of code
}
```

**Optional Modification (for better coordination):**
```csharp
// ADD at top of class (around line 42):
public NPCInteractionDetector npcDetector; // Optional reference

// MODIFY Update() method (around line 76):
void Update()
{
    Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, 3f, itemLayer))
    {
        currentItem = hit.collider.gameObject;
        interactionUI.SetActive(true);
        
        // OPTIONAL: Hide NPC prompt when item interaction is active
        if (npcDetector != null)
        {
            npcDetector.HideInteractionPrompt();
        }
        // ... rest of code remains the same
    }
    // ... rest of code
}
```

**Note:** This is optional - the systems can work independently if preferred.

---

## 🆕 NEW SCRIPTS - CODE STRUCTURE PREVIEW

### **NPCInteractionDetector.cs - Core Structure**

**Location:** `Unity Project/Building Project P.3/Assets/Scripts/NPCInteractionDetector.cs`

```csharp
/*This script handles NPC interaction detection, similar to ItemCollector pattern*/
using UnityEngine;
using TMPro;

public class NPCInteractionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public Camera playerCamera;
    public LayerMask npcLayer;
    public float detectionRange = 5f;
    
    [Header("UI References")]
    public GameObject interactionPromptUI; // "Press E to Talk" text
    public TMP_Text promptText;
    
    [Header("NPC Reference")]
    public NPCController currentNPC;
    
    private bool isInteracting = false;
    
    void Update()
    {
        if (isInteracting) return; // Don't detect while chatting
        
        // Raycast detection (same pattern as ItemCollector)
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, detectionRange, npcLayer))
        {
            NPCController npc = hit.collider.GetComponent<NPCController>();
            if (npc != null)
            {
                currentNPC = npc;
                ShowInteractionPrompt();
                
                // Check for E key press
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnInteract();
                }
            }
        }
        else
        {
            HideInteractionPrompt();
            currentNPC = null;
        }
    }
    
    public void ShowInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }
    
    public bool CanInteract()
    {
        return currentNPC != null && !isInteracting;
    }
    
    public void OnInteract()
    {
        if (currentNPC != null)
        {
            isInteracting = true;
            HideInteractionPrompt();
            currentNPC.StartInteraction();
        }
    }
    
    public void OnInteractionEnd()
    {
        isInteracting = false;
    }
}
```

---

### **NPCController.cs - Core Structure**

**Location:** `Unity Project/Building Project P.3/Assets/Scripts/NPCController.cs`

```csharp
/*This script handles NPC behavior, animations, and interaction state*/
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("References")]
    public Animator npcAnimator;
    public GameObject player;
    public NPCChatUI chatUI;
    
    [Header("Settings")]
    public float interactionRange = 3f;
    public string npcName = "University Assistant";
    
    private bool isInteracting = false;
    private bool playerInRange = false;
    
    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            if (playerInRange && !wasInRange)
            {
                OnPlayerEnterRange();
            }
            else if (!playerInRange && wasInRange)
            {
                OnPlayerExitRange();
            }
        }
    }
    
    void OnPlayerEnterRange()
    {
        // Play greeting animation
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("Greet");
        }
    }
    
    void OnPlayerExitRange()
    {
        if (!isInteracting)
        {
            // Return to idle
            if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("Idle");
            }
        }
    }
    
    public void StartInteraction()
    {
        isInteracting = true;
        
        // Play talking animation
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("IsTalking", true);
        }
        
        // Open chat UI
        if (chatUI != null)
        {
            chatUI.OpenChat(this);
        }
    }
    
    public void EndInteraction()
    {
        isInteracting = false;
        
        // Stop talking animation
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("IsTalking", false);
        }
        
        // Close chat UI
        if (chatUI != null)
        {
            chatUI.CloseChat();
        }
    }
}
```

---

## 🔗 INTEGRATION FLOW DIAGRAM

```
┌─────────────────────────────────────────────────────────┐
│                    PLAYER GAMEOBJECT                      │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Movement_Player.cs (NO CHANGES)                 │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  NPCInteractionDetector.cs (NEW)                  │   │
│  │  - Raycast detection                              │   │
│  │  - Shows "Press E" prompt                         │   │
│  │  - Calls NPCController on E press                 │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          │
                          │ (E key press)
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    NPC GAMEOBJECT                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  NPCController.cs (NEW)                           │   │
│  │  - Manages animations                             │   │
│  │  - Handles interaction state                      │   │
│  │  - Opens chat UI                                  │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          │
                          │ (Opens chat)
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    CHAT UI CANVAS                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │  NPCChatUI.cs (NEW)                               │   │
│  │  - Manages chat interface                         │   │
│  │  - Sends messages to GeminiAPIClient              │   │
│  │  - Displays responses                             │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          │
                          │ (User message)
                          ↓
┌─────────────────────────────────────────────────────────┐
│              GeminiAPIClient.cs (NEW)                     │
│  - Formats prompt with context                           │
│  - Sends to Gemini API                                   │
│  - Returns response                                      │
└─────────────────────────────────────────────────────────┘
                          │
                          │ (Gets context)
                          ↓
┌─────────────────────────────────────────────────────────┐
│           VectorStoreManager.cs (NEW)                      │
│  - Searches university data                              │
│  - Returns relevant context                              │
│  - Reads from CSV files                                  │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 KEY INTEGRATION POINTS SUMMARY

### **1. Input Handling Priority:**
- **NPC interaction** (NEW) → Highest priority when near NPC
- **Menu toggle** (EXISTING) → Falls back if no NPC interaction
- **Item collection** (EXISTING) → Independent system

### **2. Layer System:**
- Create new **"NPC"** layer in Unity
- Assign NPC GameObject to this layer
- Set `npcLayer` in NPCInteractionDetector
- No conflicts with existing layers

### **3. UI Hierarchy:**
```
Canvas (Existing)
├── Main UI (Existing)
├── Minimap (Existing)
├── Menu (Existing)
└── NPC Chat (NEW)
    ├── ChatPanel
    ├── MessageHistory
    ├── InputField
    └── SendButton
```

### **4. Component References:**
- **Player GameObject:**
  - `Movement_Player` (existing)
  - `NPCInteractionDetector` (NEW)
  
- **NPC GameObject:**
  - `NPCController` (NEW)
  - `Animator` (NEW)
  - `Collider` (for detection)
  
- **WaypointSystem GameObject:**
  - `WaypointSystem` (modified)
  - Reference to `NPCInteractionDetector` (NEW)

---

## ✅ MINIMAL INVASIVENESS CHECKLIST

- [x] No changes to Movement_Player.cs
- [x] Minimal changes to WaypointSystem.cs (only E key priority)
- [x] Optional changes to ItemCollector.cs (coordination only)
- [x] New scripts are self-contained
- [x] Uses existing input system
- [x] Uses existing UI patterns
- [x] Can be disabled easily (set NPCInteractionEnabled = false)
- [x] No changes to existing data files
- [x] New layer doesn't conflict with existing layers

---

## 🚨 IMPORTANT NOTES

1. **E Key Conflict Resolution:**
   - NPC interaction takes priority when player is near NPC
   - Menu toggle only happens if no NPC interaction is available
   - This is handled by the priority check in WaypointSystem

2. **Mobile Support:**
   - NPCInteractionDetector will need mobile input handling
   - Can use same pattern as ItemCollector (touch input)

3. **Performance:**
   - Raycast runs every frame (like ItemCollector)
   - API calls are async/coroutine-based
   - Vector search should be optimized (caching, etc.)

4. **Error Handling:**
   - API failures should show user-friendly messages
   - Network issues should be handled gracefully
   - Missing NPC references should log warnings

---

**End of Code Integration Guide**


