# ⚡ Quick Setup Guide - First Steps

## ✅ What We've Created So Far

1. ✅ **NPCInteractionDetector.cs** - Detects NPCs and handles E key
2. ✅ **NPCController.cs** - Manages NPC behavior

## 🎯 Next Steps in Unity Editor

### **STEP 1: Create NPC Layer**

1. Open Unity Editor
2. Go to **Edit → Project Settings → Tags and Layers**
3. Under **Layers**, find an empty slot (e.g., Layer 8)
4. Name it **"NPC"**
5. Click **Save**

### **STEP 2: Create NPC GameObject (Placeholder)**

1. In your scene, right-click in Hierarchy
2. Create Empty GameObject → Name it **"NPC_Assistant"**
3. Add a **Capsule** or **Cube** as child (temporary visual)
4. Position it where you want the NPC (e.g., in a hallway or lobby)
5. Add **Box Collider** component to the NPC GameObject
6. Set the **Layer** to **"NPC"** (the one you just created)

### **STEP 3: Add NPCController Script**

1. Select the **NPC_Assistant** GameObject
2. Click **Add Component**
3. Search for **"NPC Controller"**
4. Add it
5. In the inspector:
   - Set **NPC Name** to "University Assistant" (or whatever you want)
   - **Player** field: Drag your Player GameObject from hierarchy
   - **Chat UI**: Leave empty for now (we'll add this next)

### **STEP 4: Add NPCInteractionDetector to Player**

1. Select your **Player** GameObject in hierarchy
2. Click **Add Component**
3. Search for **"NPC Interaction Detector"**
4. Add it
5. In the inspector:
   - **Player Camera**: Drag your main camera (or leave empty, it will find Main Camera)
   - **NPC Layer**: Click the dropdown, select **"NPC"** layer
   - **Detection Range**: Set to 5 (or adjust as needed)
   - **Interaction Prompt UI**: Leave empty for now (we'll create this next)

### **STEP 5: Create Simple "Press E" Prompt UI**

1. In your Canvas (or create a new Canvas for NPC UI):
   - Right-click Canvas → **UI → Text - TextMeshPro** (or regular Text)
   - Name it **"NPC_Prompt"**
   - Set text to **"Press E to Talk"**
   - Position it (e.g., center-bottom of screen, or above NPC)
   - Style it as you like (color, size, etc.)
2. **Disable** the GameObject (uncheck in inspector)
3. Drag it to **Interaction Prompt UI** field in NPCInteractionDetector

### **STEP 6: Test It!**

1. **Play** the scene
2. Walk up to the NPC GameObject
3. Look at it (center of screen should be on NPC)
4. You should see **"Press E to Talk"** appear
5. Press **E**
6. Check Console - you should see: *"Started interaction with University Assistant. Chat UI not yet implemented."*

---

## 🐛 Troubleshooting

### **"Press E" prompt doesn't appear:**
- ✅ Check NPC GameObject has **Box Collider**
- ✅ Check NPC GameObject **Layer** is set to "NPC"
- ✅ Check NPCInteractionDetector **NPC Layer** mask includes "NPC"
- ✅ Check NPC GameObject has **NPCController** component
- ✅ Check you're looking at the NPC (center of screen)
- ✅ Check **Detection Range** is large enough (try 10)

### **E key doesn't work:**
- ✅ Check Console for errors
- ✅ Make sure NPCController component is on the NPC
- ✅ Check NPC GameObject is on "NPC" layer

### **Scripts not showing in Add Component:**
- ✅ Make sure scripts are in `Assets/Scripts/` folder
- ✅ Unity might need to compile - wait a moment
- ✅ Check for compilation errors in Console

---

## 📝 What's Next?

Once you've tested the basic interaction:

1. **Modify WaypointSystem.cs** - Make E key priority work
2. **Create Chat UI** - Visual chat interface
3. **Add Animations** - NPC animations (optional)
4. **Add LLM Integration** - Connect to Gemini API

---

## 🎨 Optional: Better NPC Visual

Instead of a capsule/cube, you can:
- Import a character model from Asset Store
- Use a simple humanoid placeholder
- Add a simple animated character

For now, a capsule works fine for testing!

---

**Ready to continue? Let me know when you've tested this and we'll move to the next step!**

