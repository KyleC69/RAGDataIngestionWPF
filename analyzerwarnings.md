Solution RAGDataIngestionWPF.slnx
    Project DataIngestionLib
      src\DataIngestionLib\Agents\AgentFactory.cs:14 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\Agents\AgentFactory.cs:43 Field '_contextCacheRecorder' is assigned but its value is never used
      src\DataIngestionLib\Agents\AgentFactory.cs:46 Field '_ragContextInjector' is assigned but its value is never used
      src\DataIngestionLib\Agents\AgentFactory.cs:53 Name 'disposedValue' does not match rule 'Instance fields (private)'. Suggested name is '_disposedValue'.
      src\DataIngestionLib\Agents\AgentFactory.cs:213 'GC.SuppressFinalize' is invoked for type without destructor
      src\DataIngestionLib\Contracts\HistoryIdentity.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IChatBusyStateScopeFactory.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IChatConversationService.cs:17 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IChatHistoryMemoryProvider.cs:16 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IChatHistoryProvider.cs:16 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IContextCitationFormatter.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationBudgetEvaluator.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationBudgetEventPublisher.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationContextCacheStore.cs:17 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationHistoryContextOrchestrator.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationHistoryLoader.cs:17 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationProgressLogService.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationProgressLogStore.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationSessionBootstrapper.cs:17 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IConversationSessionBootstrapper.cs:32 Name 'Agent' does not match rule 'Parameters'. Suggested name is 'agent'.
      src\DataIngestionLib\Contracts\IConversationSessionBootstrapper.cs:32 Name 'Session' does not match rule 'Parameters'. Suggested name is 'session'.
      src\DataIngestionLib\Contracts\IConversationSessionBootstrapper.cs:32 Name 'ConversationId' does not match rule 'Parameters'. Suggested name is 'conversationId'.
      src\DataIngestionLib\Contracts\IConversationTokenCounter.cs:17 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IRagContextMessageAssembler.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IRagContextOrchestrator.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IRagContextSource.cs:16 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IRagRetrievalService.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\ISQLChatHistoryProvider.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\ISqlVectorStore.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\IVectorChatHistoryProvider.cs:16 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\Contracts\TokenBudget.cs:10 Namespace does not correspond to file location, must be: 'DataIngestionLib.Contracts'
      src\DataIngestionLib\DocIngestion\IngestionPipeline.cs:9 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\DocIngestion\IngestionPipeline.cs:10 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\DocIngestion\IngestionPipeline.cs:11 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\DocIngestion\IngestionPipeline.cs:12 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\DocIngestion\LearningHtmlRunner.cs:58 Parameter 'settings' has no matching param tag in the XML comment for DataIngestionLib.DocIngestion.LearningHtmlRunner.LearningHtmlRunner (but other parameters do)
      src\DataIngestionLib\DocIngestion\LearningHtmlRunner.cs:467 Short-lived 'HttpClient' is not recommended. Frequently creating 'HttpClient' instances can lead to socket exhaustion. Consider using 'IHttpClientFactory' or a long-lived (e.g., static) 'HttpClient' instance instead.
      src\DataIngestionLib\HistoryModels\ChatHistoryMessage.cs:11 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\HistoryModels\ChatHistoryMessage.cs:16 Namespace does not correspond to file location, must be: 'DataIngestionLib.HistoryModels'
      src\DataIngestionLib\HistoryModels\ChatHistoryMessage.cs:29 Possible performance issues caused by unlimited string length
      src\DataIngestionLib\HistoryModels\ChatHistoryMessage.cs:40 Possible performance issues caused by unlimited string length
      src\DataIngestionLib\HistoryModels\ChatHistoryMessageExtensions.cs:20 Expression is always true according to nullable reference types' annotations
      src\DataIngestionLib\HistoryModels\ChatHistoryMessageExtensions.cs:44 Redundant 'switch' expression arm
      src\DataIngestionLib\HistoryModels\ChatHistoryTextChunk.cs:15 Namespace does not correspond to file location, must be: 'DataIngestionLib.HistoryModels'
      src\DataIngestionLib\HistoryModels\ChatHistoryTextChunk.cs:33 Possible performance issues caused by unlimited string length
      src\DataIngestionLib\Models\AIMessage.cs:33 Name '_authorName' does not match rule 'Instance fields (not private)'. Suggested name is 'AuthorName'.
      src\DataIngestionLib\Models\AIMessage.cs:35 Name '_contents' does not match rule 'Instance fields (not private)'. Suggested name is 'Contents'.
      src\DataIngestionLib\Models\AIMessage.cs:110 Property 'ContentForDebuggerDisplay' is never used
      src\DataIngestionLib\Models\AIMessage.cs:132 Property 'EllipsesForDebuggerDisplay' is never used
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:26 Name 'providerInputFilter' does not match rule 'Static readonly fields (private)'. Suggested name is 'ProviderInputFilter'.
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:27 Name 'storeInputRequestFilter' does not match rule 'Static readonly fields (private)'. Suggested name is 'StoreInputRequestFilter'.
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:28 Name 'storeInputResponseFilter' does not match rule 'Static readonly fields (private)'. Suggested name is 'StoreInputResponseFilter'.
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:53 Cannot resolve symbol 'InvokedContext'
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:67 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:69 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:87 Cannot resolve symbol 'InvokingContext'
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:101 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:102 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:125 Cannot resolve symbol 'InvokingContext'
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:139 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:158 Cannot resolve symbol 'InvokedContext'
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:171 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ChatHistoryContextInjector.cs:176 Redundant control flow jump statement
      src\DataIngestionLib\Providers\ConversationCacheContextSource.cs:69 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ConversationCacheContextSource.cs:88 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\ConversationContextCacheRecorder.cs:149 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:26 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:39 The field is always assigned before being used and can be converted into a local variable
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:70 Parameter 'currentSession' is never used
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:188 Local variable 'task' is never used
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:331 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:332 'GC.SuppressFinalize' is invoked for type without destructor
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:453 Cannot resolve symbol 'conversationId'
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:536 Method 'GetSessionStateValue' is never used
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:627 Cannot resolve symbol 'InvokingContext'
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:648 Method 'ProcessPersistedMessages' is never used
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:648 Parameter 'cancellationToken' is never used
      src\DataIngestionLib\Providers\SqlChatHistoryProvider.cs:654 Content of collection 'persistedMessages' is never updated
      src\DataIngestionLib\Services\ChatConversationService.cs:34 Field '_appSettings' is assigned but its value is never used
      src\DataIngestionLib\Services\ChatConversationService.cs:179 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ChatConversationService.cs:421 Usage of <inheritdoc /> is invalid: No base candidate to inherit from
      src\DataIngestionLib\Services\ChatConversationService.cs:444 Usage of <inheritdoc /> is invalid: No base candidate to inherit from
      src\DataIngestionLib\Services\ChatConversationService.cs:447 Usage of <inheritdoc /> is invalid: No base candidate to inherit from
      src\DataIngestionLib\Services\ContextCitationFormatter.cs:37 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ContextCitationFormatter.cs:74 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationAgentRunner.cs:32 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationHistoryContextOrchestrator.cs:26 Field '_appSettings' is assigned but its value is never used
      src\DataIngestionLib\Services\ConversationHistoryContextOrchestrator.cs:27 Field '_citationFormatter' is assigned but its value is never used
      src\DataIngestionLib\Services\ConversationHistoryContextOrchestrator.cs:28 Field '_historyLoader' is assigned but its value is never used
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:37 Field '_appSettings' is assigned but its value is never used
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:58 Parameter 'settings' has no matching param tag in the XML comment for DataIngestionLib.Services.ConversationHistoryLoader.ConversationHistoryLoader (but other parameters do)
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:74 Cannot resolve parameter 'conversationId'
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:90 Cannot resolve symbol 'conversationId'
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:92 Parameter 'identity' has no matching param tag in the XML comment for DataIngestionLib.Services.ConversationHistoryLoader.LoadConversationHistoryAsync (but other parameters do)
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:94 Expression is always false according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationHistoryLoader.cs:110 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationProgressLogService.cs:85 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationProgressLogService.cs:91 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationProgressLogService.cs:103 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationProgressLogService.cs:153 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationProgressLogService.cs:160 '??' left operand is never 'null' according to nullable reference types' annotations
      src\DataIngestionLib\Services\ConversationSessionBootstrapper.cs:37 Name 'INITIALIZATION_LOCK_TIMEOUT' does not match rule 'Static readonly fields (private)'. Suggested name is 'InitializationLockTimeout'.
      src\DataIngestionLib\Services\ConversationSessionBootstrapper.cs:108 'GC.SuppressFinalize' is invoked for type without destructor
      src\DataIngestionLib\Services\ConversationSessionBootstrapper.cs:142 Parameter 'currentSession' is never used
      src\DataIngestionLib\Services\ConversationSessionBootstrapper.cs:232 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationContextCacheStore.cs:114 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationContextCacheStore.cs:120 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationContextCacheStore.cs:194 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationContextCacheStore.cs:285 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationContextCacheStore.cs:321 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\FileConversationProgressLogStore.cs:49 Value assigned is not used in any execution path
      src\DataIngestionLib\Services\FileConversationProgressLogStore.cs:220 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\LoggingMessages.cs:28 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:38 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:48 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:58 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:58 Name 'KeyPath' does not match rule 'Parameters'. Suggested name is 'keyPath'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'Title' does not match rule 'Parameters'. Suggested name is 'title'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'Description' does not match rule 'Parameters'. Suggested name is 'description'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'DocumentId' does not match rule 'Parameters'. Suggested name is 'documentId'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'UpdatedAt' does not match rule 'Parameters'. Suggested name is 'updatedAt'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'MsDate' does not match rule 'Parameters'. Suggested name is 'msDate'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'OgUrl' does not match rule 'Parameters'. Suggested name is 'ogUrl'.
      src\DataIngestionLib\Services\LoggingMessages.cs:68 Name 'Summary' does not match rule 'Parameters'. Suggested name is 'summary'.
      src\DataIngestionLib\Services\LoggingMessages.cs:98 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:98 Name 'KeyPath' does not match rule 'Parameters'. Suggested name is 'keyPath'.
      src\DataIngestionLib\Services\LoggingMessages.cs:108 Name 'Message' does not match rule 'Parameters'. Suggested name is 'message'.
      src\DataIngestionLib\Services\LoggingMessages.cs:108 Name 'KeyPath' does not match rule 'Parameters'. Suggested name is 'keyPath'.
      src\DataIngestionLib\Services\RagContextMessageAssembler.cs:69 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\Services\RagQueryExpander.cs:61 Conditional access qualifier expression is never null according to nullable reference types' annotations
      src\DataIngestionLib\ToolFunctions\ServiceHealthTool.cs:52 Qualifier is redundant
      src\DataIngestionLib\ToolFunctions\ToolBuilder.cs:46 Local variable 'processSnapshotTool' is never used
      src\DataIngestionLib\Utils\Vectorizer.cs:9 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\Utils\Vectorizer.cs:10 Using directive is not required by the code and can be safely removed
      src\DataIngestionLib\Utils\Vectorizer.cs:11 Using directive is not required by the code and can be safely removed
    
    Project RAGDataIngestionWPF
      src\RAGDataIngestionWPF\App.xaml.cs:54 Base type 'Application' is already specified in other parts
      src\RAGDataIngestionWPF\App.xaml.cs:165 Method 'GetAppLocation' is never used
      src\RAGDataIngestionWPF\Helpers\MarkdownFlowDocumentFormatter.cs:258 Expression is always false
      src\RAGDataIngestionWPF\Helpers\MarkdownFlowDocumentFormatter.cs:259 Code is heuristically unreachable
      src\RAGDataIngestionWPF\Settings.cs:1 Namespace does not correspond to file location, must be: 'RAGDataIngestionWPF'
      src\RAGDataIngestionWPF\Settings.cs:11 Empty constructor is redundant. The compiler generates the same by default.
      src\RAGDataIngestionWPF\Settings.cs:20 Method 'SettingChangingEventHandler' is never used
      src\RAGDataIngestionWPF\Settings.cs:20 Parameter 'sender' is never used
      src\RAGDataIngestionWPF\Settings.cs:20 Parameter 'e' is never used
      src\RAGDataIngestionWPF\Settings.cs:24 Method 'SettingsSavingEventHandler' is never used
      src\RAGDataIngestionWPF\Settings.cs:24 Parameter 'sender' is never used
      src\RAGDataIngestionWPF\Settings.cs:24 Parameter 'e' is never used
      src\RAGDataIngestionWPF\ViewModels\BaseViewModel.cs:24 Base interface 'INotifyPropertyChanged' is redundant because RAGDataIngestionWPF.ViewModels.BaseViewModel inherits 'ObservableObject'
      src\RAGDataIngestionWPF\ViewModels\BaseViewModel.cs:24 Base interface 'INotifyPropertyChanging' is redundant because RAGDataIngestionWPF.ViewModels.BaseViewModel inherits 'ObservableObject'
      src\RAGDataIngestionWPF\ViewModels\DataGridViewModel.cs:33 Field '_logger' is assigned but its value is never used
      src\RAGDataIngestionWPF\ViewModels\LogInViewModel.cs:30 Name 'statusMessage' does not match rule 'Instance fields (private)'. Suggested name is '_statusMessage'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:40 Name 'ragTokenCount' does not match rule 'Instance fields (private)'. Suggested name is '_ragTokenCount'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:41 Name 'selectedTaskPlan' does not match rule 'Instance fields (private)'. Suggested name is '_selectedTaskPlan'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:42 Name 'sessionTokenCount' does not match rule 'Instance fields (private)'. Suggested name is '_sessionTokenCount'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:43 Name 'systemTokenCount' does not match rule 'Instance fields (private)'. Suggested name is '_systemTokenCount'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:44 Name 'toolTokenCount' does not match rule 'Instance fields (private)'. Suggested name is '_toolTokenCount'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:45 Name 'totalTokenCount' does not match rule 'Instance fields (private)'. Suggested name is '_totalTokenCount'.
      src\RAGDataIngestionWPF\ViewModels\MainViewModel.cs:257 Possible 'null' assignment to non-nullable entity
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:45 Name 'applicationId' does not match rule 'Instance fields (private)'. Suggested name is '_applicationId'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:47 Name 'chatHistoryConnectionString' does not match rule 'Instance fields (private)'. Suggested name is '_chatHistoryConnectionString'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:49 Name 'chatHistoryContextEnabled' does not match rule 'Instance fields (private)'. Suggested name is '_chatHistoryContextEnabled'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:51 Name 'chatHistorySettingsStatus' does not match rule 'Instance fields (private)'. Suggested name is '_chatHistorySettingsStatus'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:53 Name 'chatModelName' does not match rule 'Instance fields (private)'. Suggested name is '_chatModelName'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:55 Name 'embeddingsModelName' does not match rule 'Instance fields (private)'. Suggested name is '_embeddingsModelName'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:57 Name 'maxContextMessages' does not match rule 'Instance fields (private)'. Suggested name is '_maxContextMessages'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:59 Name 'maxContextTokens' does not match rule 'Instance fields (private)'. Suggested name is '_maxContextTokens'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:62 Name 'minimumLogLevel' does not match rule 'Instance fields (private)'. Suggested name is '_minimumLogLevel'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:64 Name 'rAGKnowledgeEnabled' does not match rule 'Instance fields (private)'. Suggested name is '_rAgKnowledgeEnabled'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:66 Name 'theme' does not match rule 'Instance fields (private)'. Suggested name is '_theme'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:68 Name 'user' does not match rule 'Instance fields (private)'. Suggested name is '_user'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:70 Name 'versionDescription' does not match rule 'Instance fields (private)'. Suggested name is '_versionDescription'.
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:308 Conditional access qualifier expression is known to be not null
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:308 '??' left operand is never 'null'
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:309 Conditional access qualifier expression is known to be not null
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:309 '??' left operand is never 'null'
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:310 Conditional access qualifier expression is known to be not null
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:310 '??' left operand is never 'null'
      src\RAGDataIngestionWPF\ViewModels\SettingsViewModel.cs:340 Local variable 'theme' hides field 'RAGDataIngestionWPF.ViewModels.SettingsViewModel.theme'
      src\RAGDataIngestionWPF\ViewModels\ShellViewModel.cs:35 Name 'selectedMenuItem' does not match rule 'Instance fields (private)'. Suggested name is '_selectedMenuItem'.
      src\RAGDataIngestionWPF\ViewModels\ShellViewModel.cs:37 Name 'selectedOptionsMenuItem' does not match rule 'Instance fields (private)'. Suggested name is '_selectedOptionsMenuItem'.
      src\RAGDataIngestionWPF\ViewModels\ShellViewModel.cs:182 Possible 'null' assignment to non-nullable entity
      src\RAGDataIngestionWPF\ViewModels\UserViewModel.cs:25 Name 'name' does not match rule 'Instance fields (private)'. Suggested name is '_name'.
      src\RAGDataIngestionWPF\ViewModels\UserViewModel.cs:27 Name 'photo' does not match rule 'Instance fields (private)'. Suggested name is '_photo'.
      src\RAGDataIngestionWPF\ViewModels\UserViewModel.cs:29 Name 'userPrincipalName' does not match rule 'Instance fields (private)'. Suggested name is '_userPrincipalName'.
      src\RAGDataIngestionWPF\ViewModels\WebViewViewModel.cs:36 Name 'failedMesageVisibility' does not match rule 'Instance fields (private)'. Suggested name is '_failedMesageVisibility'.
      src\RAGDataIngestionWPF\ViewModels\WebViewViewModel.cs:38 Name 'isLoadingVisibility' does not match rule 'Instance fields (private)'. Suggested name is '_isLoadingVisibility'.
      src\RAGDataIngestionWPF\ViewModels\WebViewViewModel.cs:40 Name 'source' does not match rule 'Instance fields (private)'. Suggested name is '_source'.
      src\RAGDataIngestionWPF\Views\MainPage.xaml.cs:126 Return value of iterator is not used
      src\RAGDataIngestionWPF\Views\MainPage.xaml.cs:162 Access to a static member of a type via a derived type
    
    Project RAGDataIngestionWPF.Tests.MSTest
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs: Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:3 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:9 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:10 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:11 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:12 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:14 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:22 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:26 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:27 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:40 Name '_session' does not match rule 'Instance fields (not private)'. Suggested name is 'Session'.
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:41 Name '_provider' does not match rule 'Instance fields (not private)'. Suggested name is 'Provider'.
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:68 Local variable 'appSettings' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:69 Local variable 'sqlChatHistoryProvider' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:70 Local variable 'chatHistoryContextInjector' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:75 The parameter 'agentDescription' has the same default value
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:75 The parameter 'instructions' has the same default value
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:85 Name '_agent' does not match rule 'Static fields (not private)'. Suggested name is 'Agent'.
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:89 The field is always assigned before being used and can be converted into a local variable
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:132 Parameter 'session' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:195 Local variable 'sqlChatHistoryProvider' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIChatResponseTests.cs:196 Local variable 'chatHistoryContextInjector' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\AIMessageTests.cs:96 Expression is always null
      tests\RAGDataIngestionWPF.Tests.MSTest\ChatMessageExtensionsTests.cs:72 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:12 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:13 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:100 Possible 'null' assignment to non-nullable entity
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:134 Possible 'null' assignment to non-nullable entity
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:134 Possible 'null' assignment to non-nullable entity
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:141 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:142 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:180 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:181 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:183 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:184 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:186 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:187 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:189 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConfigurationAndEdgeCaseServiceTests.cs:190 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\ConversationSessionBootstrapperTests.cs:3 Using directive is not required by the code and can be safely removed
      tests\RAGDataIngestionWPF.Tests.MSTest\CoverageBoostMiscTests.cs:97 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\CoverageBoostMiscTests.cs:103 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\MainViewModelTests.cs:40 Possible 'null' assignment to non-nullable entity
      tests\RAGDataIngestionWPF.Tests.MSTest\MainViewModelTests.cs:46 Possible 'null' assignment to non-nullable entity
      tests\RAGDataIngestionWPF.Tests.MSTest\NavigationServiceIntegrationTests.cs:130 Auto-property accessor 'LastParameter.get' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\NavigationServiceIntegrationTests.cs:176 Class 'TestPageB' is never used
      tests\RAGDataIngestionWPF.Tests.MSTest\StaTestHelper.cs:56 Method is recursive on all execution paths
      tests\RAGDataIngestionWPF.Tests.MSTest\ToolBuilderEndToEndIntegrationTests.cs:42 Use not null pattern instead of a type check succeeding on any not-null value
      tests\RAGDataIngestionWPF.Tests.MSTest\ViewModelAndConverterTests.cs:80 Possible 'System.NullReferenceException'
      tests\RAGDataIngestionWPF.Tests.MSTest\WindowsDiagnosticsIntegrationTests.cs:260 Qualifier is redundant
      tests\RAGDataIngestionWPF.Tests.MSTest\WindowsDiagnosticsIntegrationTests.cs:260 Qualifier is redundant
    
  