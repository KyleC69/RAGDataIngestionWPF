---

name: Application Components
description: high level view of components in application
author: Kyle Crowder
status: Work in progress

---

# Application Components - work in progress

## Application features several different components

### Data Ingestion: (DataGrid.xaml) DataIngestionLib/DocIngestion

- Features data ingestion from varying sources into local sql database.
- Features data grid of current chat history contents (currently for dev)

### Main page: ChatConversationService

- Agentic Chat capable of system analysis
- Includes several tool for system monitoring and editing
- Displays persisted agent task plans for the active conversation, including plan status, current step, and artifact count.
- Each chat turn now creates and advances a durable task plan automatically so long-running or cancelled work can be inspected and resumed.
- Chat history persisted to SQL server.
- Previous tool results and context enhancements are saved in file as cache.
- Cache is searched first for enhancements in next turn to speed searchs
- AIContextInjector implementation to use chat history from any conversation, can be restricted by ApplicationID, UserId, AgentID and date or time.
- AIContextInjector for local sql rag context enrichment. SQL stored procs exist for RAG.
- Long Agent task plans(planning) are saved and tracked for easy resuming of aborted or hung tasks.


### Settings page: (Appsettings) SettingsPage.xaml

- All library settings adjustable from Settings UI
- Any UI settings exposed here.

### System Monitoring

- Features full application monitoring - Future
- Full audit log of all agent activity for easy tracing and restore