# Project Guidelines

## Architecture

- Treat `src/RAGDataIngestionWPF` as the WPF composition root: host startup, DI registration, navigation, and view orchestration belong there.
- Keep AI, RAG, chat history, ingestion, and tool execution logic in `src/DataIngestionLib`; new behavior there should remain UI-agnostic.
- Keep shared UI-supporting infrastructure in `src/RAGDataIngestionWPF.Core` rather than pushing it into the WPF project or the agent library.
- Prefer extending existing service seams and interfaces in `src/DataIngestionLib/Contracts` instead of growing orchestration classes with more direct responsibilities.
- Start documentation lookup at [docs/DocumentationManifest.md](../docs/DocumentationManifest.md), then use [docs/Architecture.md](../docs/Architecture.md), [docs/ContextManagement.md](../docs/ContextManagement.md), and [docs/Components.md](../docs/Components.md) as needed.

## Build and Test

- Use Windows with the .NET 10 preview SDK; this repo targets `net10.0-windows` and `net10.0-windows10.0.19041.0`.
- Build the WPF app with `dotnet build src/RAGDataIngestionWPF/RAGDataIngestionWPF.csproj`.
- Build the core library with `dotnet build src/DataIngestionLib/DataIngestionLib.csproj`.
- Run tests with `dotnet test tests/RAGDataIngestionWPF.Tests.MSTest/RAGDataIngestionWPF.Tests.MSTest.csproj`.
- The app and some tests assume Windows-specific APIs, local SQL Server configuration, and a local Ollama setup.
- If the WPF project fails with transient generated-file errors such as missing `obj/*.g.cs` or `CS5001`, clean `src/RAGDataIngestionWPF/obj` and `bin` and rebuild before changing code.

## Code Style

- Follow `.editorconfig`: use 4-space indentation for C# and XAML, tabs for XML and project files, and preserve CRLF line endings.
- Match the existing file layout and blank-line-heavy style; avoid mass reformatting unrelated files.
- Prefer constructor injection, `ArgumentNullException.ThrowIfNull(...)`, async APIs with `CancellationToken`, and narrow interfaces for service boundaries.
- Keep constants in `UPPER_SNAKE_CASE` when they are public, internal, or otherwise covered by the repo naming rules.

## Conventions

- For startup and registration changes, update the Generic Host wiring in `src/RAGDataIngestionWPF/App.xaml.cs` and keep the WPF project as the composition root.
- For chat and context features, preserve the repository's separation between permanent SQL history, semi-volatile conversation cache, durable progress logs, and RAG sources; see [docs/ContextManagement.md](../docs/ContextManagement.md).
- Extend the current chat orchestration seams instead of re-centralizing logic in `ChatConversationService`; recent code splits responsibilities across dedicated services for startup, history loading, token state, busy state, usage logging, context citation formatting, and progress persistence.
- Use `IUserIdentityProvider` for runtime user identity in conversation paths; do not introduce new direct `Environment.UserName` reads there.
- When adding a config-backed application setting, update both `Properties/Settings.settings` and `Properties/Settings.Designer.cs` when the setting is sourced from the generated settings bag.
- Do not add tests against `Models/AIChatHistory.cs` unless project include rules change; that file is explicitly removed from compilation.
- Tests use MSTest and Moq. Favor deterministic behavior and edge-case coverage, and remember that `EventHandler<T>` mocks need both sender and payload when raised.
- The repo already contains reusable custom agents in `.github/agents` and prompts in `.github/prompts`; prefer those when they match the task instead of duplicating their purpose in ad hoc instructions.