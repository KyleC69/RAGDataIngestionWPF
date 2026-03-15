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



namespace DataIngestionLib.ToolFunctions;





/// <summary>
///     Represents a result type that encapsulates the outcome of a tool operation,
///     indicating success or failure, and optionally providing a value or an error message.
/// </summary>
/// <typeparam name="T">
///     The type of the value contained in the result, if the operation is successful.
/// </typeparam>
public sealed class ToolResult<T>
    {
    public string? Error { get; init; }
    public bool Success { get; init; }
    public T? Value { get; init; }








    public static ToolResult<T> Fail(string message)
        {
        return string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException("Failure message cannot be null or whitespace.", nameof(message))
                : new() { Success = false, Error = message };
        }








    public static ToolResult<T> Ok(T value)
        {
        ArgumentNullException.ThrowIfNull(value);
        return new() { Success = true, Value = value };
        }
    }