// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
    {

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    AIChatHistory ChatHistory { get; }





    /// <summary>
    ///     Gets the current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }



    ValueTask<AIChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    //   ChatMessage AddAssistantMessage(string responseCanceled);
    }