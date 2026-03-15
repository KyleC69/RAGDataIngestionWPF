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



using System.Net.Http;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





internal sealed class ToolBuilder
    {

    public static IList<AITool> GetAiTools()
        {
        WebSearchPlugin webSearchPlugin = new(new HttpClient());
        AgentLogger logger = new();

        SandboxEventLogReader eventLogReader = new();
        SafeCommandRunner safeCommandRunner = new(Environment.CurrentDirectory);




        return [

                AIFunctionFactory.Create(logger.LogMessage),
                AIFunctionFactory.Create(FileSystemWriterTool.WriteText),
                AIFunctionFactory.Create(FileSystemReaderTool.ReadFile),
                //AIFunctionFactory.Create(FullTextRagSearchTool.Search),
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(SystemInfoTool.GetInfo),
                AIFunctionFactory.Create(eventLogReader.ReadLog),
                AIFunctionFactory.Create(safeCommandRunner.Run)

        ];
        }
    }