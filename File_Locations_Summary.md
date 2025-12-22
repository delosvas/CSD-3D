# 📁 File Locations Summary - NPC System Implementation

## 🆕 NEW FILES TO CREATE

### **Scripts Folder:**
`Unity Project/Building Project P.3/Assets/Scripts/`

1. **`NPCController.cs`** - Main NPC behavior and animation controller
2. **`NPCInteractionDetector.cs`** - Player-side detection system (similar to ItemCollector pattern)
3. **`NPCChatUI.cs`** - Chat interface manager
4. **`GeminiAPIClient.cs`** - LLM API communication handler
5. **`VectorStoreManager.cs`** - Vector storage and context retrieval
6. **`NPCData.cs`** - ScriptableObject for NPC configuration

### **Settings Folder:**
`Unity Project/Building Project P.3/Assets/Settings/`

7. **`NPCConfig.asset`** - ScriptableObject for API keys and configuration (ADD TO .gitignore)

### **UI Prefabs (Create in Unity Editor):**
`Unity Project/Building Project P.3/Assets/` (or appropriate Prefabs folder)

8. **`NPCChatPanel.prefab`** - Chat interface UI prefab
9. **`NPCPromptUI.prefab`** - "Press E to Talk" floating text prefab

---

## ✏️ FILES TO MODIFY

### **1. WaypointSystem.cs**
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/WaypointSystem.cs`

**Changes:**
- **Line ~42:** Add new flag: `public bool NPCInteractionEnabled = true;`
- **Line ~326-339:** Modify `HandleInput()` method to check for NPC interaction BEFORE menu toggle
- **Add reference:** `public NPCInteractionDetector npcDetector;` (optional, for coordination)

**Specific Modification:**
```csharp
// Around line 332, modify the E key check:
if (Input.GetKeyDown(KeyCode.E))
{
    // First check if NPC interaction is available
    if (NPCInteractionEnabled && npcDetector != null && npcDetector.IsInteractingWithNPC())
    {
        // NPC interaction takes priority, don't open menu
        return;
    }
    
    // Original menu toggle code continues...
    if (EbeingAbledToUse == true)
    {
        // ... existing code ...
    }
}
```

### **2. ItemCollector.cs** (Optional - Minimal Change)
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/ItemCollector.cs`

**Changes:**
- **Line ~15:** Add reference: `public GameObject npcInteractionDetector;` (optional)
- **Line ~82:** Add check to hide NPC prompt when item interaction is active

**Specific Modification:**
```csharp
// Around line 82, when showing interactionUI:
if (Physics.Raycast(ray, out hit, 3f, itemLayer))
{
    // Hide NPC prompt if it's showing
    if (npcInteractionDetector != null)
    {
        npcInteractionDetector.GetComponent<NPCInteractionDetector>()?.HideInteractionPrompt();
    }
    // ... existing code ...
}
```

---

## 🎯 INTEGRATION POINTS IN EXISTING CODE

### **Movement_Player.cs**
**Location:** `Unity Project/Building Project P.3/Assets/Scripts/Movement_Player.cs`
- **NO CHANGES NEEDED** ✅
- NPC system works independently
- Movement remains unaffected

### **Input System**
**Location:** `Unity Project/Building Project P.3/Assets/InputSystem_Actions.inputactions`
- **NO CHANGES NEEDED** ✅
- Uses existing E key binding
- Can optionally use Unity Input System if desired

---

## 📂 DATA FILES (Read-Only Usage)

### **Excel Files Folder:**
`Excel Files/`

These files will be read by `VectorStoreManager.cs`:
- `TeachersInfo.csv` - For teacher information queries
- `WinterSemester.csv` - For winter semester course queries
- `SpringSemester.csv` - For spring semester course queries
- `SubjectLists.csv` - For course/subject information
- `StandardCurriculum.csv` - For curriculum questions
- `DoorNames.csv` - For room/location information

**Note:** These files are already in the project, we just need to read them for context.

---

## 🏗️ SCENE SETUP (Unity Editor)

### **In Unity Scene:**
1. **Create NPC GameObject:**
   - Add to scene at desired location
   - Add `NPCController` component
   - Add `Animator` component
   - Set Layer to "NPC" (create new layer)

2. **Create Chat UI:**
   - Add Canvas child for NPC Chat
   - Create chat panel with UI elements
   - Add `NPCChatUI` component to panel

3. **Setup Player Reference:**
   - Add `NPCInteractionDetector` component to Player GameObject
   - Assign camera reference
   - Assign NPC layer mask

4. **Configure WaypointSystem:**
   - Add `NPCInteractionDetector` reference to WaypointSystem
   - Set `NPCInteractionEnabled = true`

---

## 🔧 CONFIGURATION FILES

### **NPCConfig.asset** (ScriptableObject)
**Location:** `Unity Project/Building Project P.3/Assets/Settings/NPCConfig.asset`

**Contains:**
- Gemini API Key (keep secret!)
- API Endpoint URL
- Max context length
- Similarity threshold for vector search
- Response timeout settings

**⚠️ IMPORTANT:** Add this file to `.gitignore` to prevent committing API keys!

---

## 📋 FILE DEPENDENCY CHART

```
NPCController.cs
    ↓ (references)
NPCInteractionDetector.cs
    ↓ (triggers)
NPCChatUI.cs
    ↓ (sends queries to)
GeminiAPIClient.cs
    ↓ (gets context from)
VectorStoreManager.cs
    ↓ (reads data from)
Excel Files/*.csv
```

---

## 🎨 ASSETS NEEDED (External)

### **3D Assets:**
- NPC character model (FBX or similar)
- Character rigging (for animations)
- Animation clips:
  - Idle animation
  - Talking animation
  - Greeting animation

**Location:** Can be placed in `Unity Project/Building Project P.3/Assets/NPC/` (new folder)

### **UI Assets:**
- Chat bubble sprites (optional)
- Button sprites (can use existing UI style)
- Icons for send/close buttons

---

## 📝 SUMMARY TABLE

| File Type | Location | Action | Priority |
|-----------|----------|--------|----------|
| NPCController.cs | `Assets/Scripts/` | CREATE | High |
| NPCInteractionDetector.cs | `Assets/Scripts/` | CREATE | High |
| NPCChatUI.cs | `Assets/Scripts/` | CREATE | High |
| GeminiAPIClient.cs | `Assets/Scripts/` | CREATE | Medium |
| VectorStoreManager.cs | `Assets/Scripts/` | CREATE | Medium |
| NPCData.cs | `Assets/Scripts/` | CREATE | Low |
| WaypointSystem.cs | `Assets/Scripts/` | MODIFY | High |
| ItemCollector.cs | `Assets/Scripts/` | MODIFY (optional) | Low |
| NPCConfig.asset | `Assets/Settings/` | CREATE | Medium |
| NPCChatPanel.prefab | Unity Editor | CREATE | High |
| NPCPromptUI.prefab | Unity Editor | CREATE | High |

---

## ✅ CHECKLIST FOR IMPLEMENTATION

### **Phase 1: Core NPC System**
- [ ] Create `NPCController.cs` in Scripts folder
- [ ] Create `NPCInteractionDetector.cs` in Scripts folder
- [ ] Modify `WaypointSystem.cs` to add NPC interaction check
- [ ] Create NPC GameObject in scene
- [ ] Test detection and interaction

### **Phase 2: Chat UI**
- [ ] Create `NPCChatUI.cs` in Scripts folder
- [ ] Create chat UI prefab in Unity Editor
- [ ] Create prompt UI prefab in Unity Editor
- [ ] Test UI opening/closing

### **Phase 3: LLM Integration**
- [ ] Create `GeminiAPIClient.cs` in Scripts folder
- [ ] Create `VectorStoreManager.cs` in Scripts folder
- [ ] Create `NPCConfig.asset` in Settings folder
- [ ] Test API connectivity
- [ ] Test vector search

### **Phase 4: Polish**
- [ ] Optional: Modify `ItemCollector.cs` for coordination
- [ ] Add error handling
- [ ] Performance optimization
- [ ] Mobile testing

---

**End of File Locations Summary**


