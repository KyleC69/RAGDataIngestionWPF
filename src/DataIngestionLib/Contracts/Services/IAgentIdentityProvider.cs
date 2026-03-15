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



namespace DataIngestionLib.Contracts.Services;





/// <summary>
///     Provides the identity of the active AI agent for session scoping and history attribution.
/// </summary>
/// <remarks>
///     Injecting this interface instead of hard-coding an agent ID string keeps the agent identity
///     configurable and testable. Implementations may resolve the ID from configuration, from the
///     agent framework, or from any other appropriate source.
/// </remarks>
public interface IAgentIdentityProvider
    {
    /// <summary>
    ///     Returns the identifier of the active agent.
    /// </summary>
    /// <returns>
    ///     A non-null, non-empty string that uniquely identifies the agent within the application.
    /// </returns>
    string GetAgentId();
    }