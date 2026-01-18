# University Assistant - Python Backend Server

This is the Python backend server for the University Assistant NPC in the Unity 3D campus project.
It provides RAG (Retrieval-Augmented Generation) powered answers using LangChain, ChromaDB, and Google Gemini. Currently only tested locally.

## Architecture

```
Unity WebGL (Client) → POST /api/chat → Python FastAPI (Server) → LangChain/ChromaDB (Context) → Google Gemini (LLM) → Response
```

## Prerequisites

- Python 3.10 or higher
- Google Gemini API Key 

## Quick Start

### 1. Create Virtual Environment

```bash
# Windows
python -m venv venv
venv\Scripts\activate

# macOS/Linux
python3 -m venv venv
source venv/bin/activate
```

### 2. Install Dependencies

```bash
pip install -r requirements.txt
```

### 3. Configure API Key

Edit the `.env` file and add your Google Gemini API key:

```
GOOGLE_API_KEY=your_actual_api_key_here
```

**IMPORTANT:** Never commit your actual API key to Git!

### 4. Add University Data

Place your university information in the `data/` folder as `.txt` files.
The server will automatically load all `.txt` files and create vector embeddings.

Example file: `data/university_info.txt`

### 5. Run the Server

```bash
python main.py
```

Or using uvicorn directly:

```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

The server will start at: http://localhost:8000

## API Endpoints

### `GET /` - Root
Returns server status and links to docs.

### `GET /health` - Health Check
Returns server health status and whether the AI brain is initialized.

### `POST /api/chat` - Chat Endpoint
Main endpoint for Unity NPC communication.

**Request:**
```json
{
    "text": "Where is the Computer Science department located?"
}
```

**Response:**
```json
{
    "response": "The Computer Science Department is located at the Voutes campus in Heraklion, Crete, Greece."
}
```

## API Documentation

Once the server is running, visit:
- **Swagger UI:** http://localhost:8000/docs
- **ReDoc:** http://localhost:8000/redoc

## Project Structure

```
PythonServer/
├── main.py              # FastAPI server with endpoints
├── rag_manager.py       # RAG logic (LangChain + ChromaDB + Gemini)
├── requirements.txt     # Python dependencies
├── .env                 # API key 
├── .gitignore          # Git ignore rules
├── README.md           # This file
└── data/               # University information files
    └── university_info.txt
```

## Configuration

### Server Settings (main.py)
- `host`: Default `0.0.0.0` (all interfaces)
- `port`: Default `8000`
- `reload`: Auto-reload on code changes (development)

### RAG Settings (rag_manager.py)
- `chunk_size`: Size of text chunks for embedding (500)
- `chunk_overlap`: Overlap between chunks (50)
- `k`: Number of relevant chunks to retrieve (3)
- `temperature`: LLM creativity (0.3 - more focused)

## Security Notes

1. **API Key:** Never hardcode or commit your API key
2. **.env file:** Added to .gitignore to prevent accidental commits

## Adding More Data

1. Add formated `.txt` files to the `data/` folder
2. Restart the server
3. The new data will be automatically embedded and available for queries

## Integration with Unity

The Unity `GeminiAPIClient.cs` is configured to call this server at `http://localhost:8000/api/chat`.
Make sure both the server and Unity are running for the NPC chat to work.
