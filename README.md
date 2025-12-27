# Quade #
v1.0

**Quade** is an experimental chatbot intended for casual, speculative and intellectual conversation. It uses *modes* to promote flexibility and provide a responsive style of speech and analysis. Mode determines the system prompt for the current generation. The aim of Quade is to serve as an alternative to the increasingly task- and information-focused styles of the official chatbots.

Quade is built in Avalonia and uses Anthropic and OpenAI models to converse, think about messages, summarize and store memories. All generation and data storage uses external APIs. The application must be configured with at least one API key for a language model provider before it can be used (Settings → Integrations). Quade works with the user's own accounts and token budgets.

Optional vector memory can be configured using:
- **Qdrant** (recommended) - Supports `text-embedding-3-large` model with 3072 dimensions for maximum quality
- **Supabase** - Supports `text-embedding-3-small` model with 1536 dimensions (limited by pgvector constraints)

Quade employs a chain of micro-prompts to classify the most recent message for mode selection. The console (View -> Show Thought Process) displays these in real-time, as well as system prompts and the memory storage process. Current available modes are Empower, Investigate, Opine, Critique and Amuse; these are identified in the client UI by a kanji sign. Yes, those are five modes. The name was chosen to reflect "four-mode Claude," but alas that is yesterday's bot; I will change it soon to reflect the current state of the project.

The conversation context is 8 exchanges plus any retrieved memories. 

Future feature work will include a Google Drive integration to sync conversations across devices, more model and database options, mobile support, and markdown support (pending a stable version of `Markdown.Avalonia` version 11.0.3).

The behavior of modes and memory are in active development so check back for updates!
