# Quade #
v1.0

**Quade** is an experimental chatbot intended for casual, speculative and intellectual conversation. It uses *modes* to promote flexibility and provide a style of speech and analysis appropriate to the topic or focus. The choice of mode determines the system prompt for the current generation. The aim of this application is to provide an alternative to the increasingly task- and information-focused styles of the official chatbots.

Quade is built in Avalonia, which uses Anthropic and OpenAI models to converse, think about messages, summarize and store memories. All generation and data storage uses the APIs. The application must be configured with at least one API key for a language model provider before it can be used. Quade works with the user's own accounts and token budgets.

Optional vector memory can be configured using:
- **Qdrant** (recommended) - Supports `text-embedding-3-large` model with 3072 dimensions for maximum quality
- **Supabase** - Supports `text-embedding-3-small` model with 1536 dimensions (limited by pgvector constraints)

Quade employs a mode selection chain to decide what style to respond with. Current available modes are Empower, Investigate, Opine, Critique and Amuse; these are identified in the client UI by a kanji sign. Yes, that is five modes. Yes, the name was chosen to reflect "four-mode Claude" but alas that is behind us now; I will change it later.

Future feature work will include a Google Drive integration to sync conversations across devices, as well as more model and database options, and mobile support.

The behavior of modes and memory are in active development so check back for updates!
