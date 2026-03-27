// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ToolBuilderEndToEndIntegrationTests.cs
// Author: Kyle L. Crowder
// Build Num: 073106



#nullable enable

using System.Collections;
using System.Reflection;
using System.Text.Json;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
[TestCategory("Integration")]
public class ToolBuilderEndToEndIntegrationTests
{

    private static void AssertJsonToolResultSucceeded(JsonElement result, string failureMessage)
    {
        Assert.AreEqual(JsonValueKind.Object, result.ValueKind, $"Expected JSON object result but found '{result.ValueKind}'.");

        JsonElement successElement = GetRequiredProperty(result, "Success");
        var success = successElement.ValueKind switch
        {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => throw new AssertFailedException($"Property 'Success' was not a JSON boolean. Actual kind: {successElement.ValueKind}.")
        };

        var error = TryGetProperty(result, "Error") is JsonElement errorElement && errorElement.ValueKind != JsonValueKind.Null ? errorElement.ToString() : null;

        Assert.IsTrue(success, string.IsNullOrWhiteSpace(error) ? failureMessage : error);
    }








    private static void AssertToolResultSucceeded(object? result, string failureMessage)
    {
        Assert.IsNotNull(result, failureMessage);

        if (result is JsonElement jsonElement)
        {
            AssertJsonToolResultSucceeded(jsonElement, failureMessage);
            return;
        }

        PropertyInfo? successProperty = result.GetType().GetProperty("Success", BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo? errorProperty = result.GetType().GetProperty("Error", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(successProperty, $"Result type '{result.GetType().FullName}' did not expose a Success property.");
        Assert.IsNotNull(errorProperty, $"Result type '{result.GetType().FullName}' did not expose an Error property.");

        var success = (bool)successProperty.GetValue(result)!;
        var error = errorProperty.GetValue(result)?.ToString();

        Assert.IsTrue(success, string.IsNullOrWhiteSpace(error) ? failureMessage : error);
    }








    private static async Task<object?> AwaitInvocationAsync(object? invocation)
    {
        if (invocation == null)
        {
            return null;
        }

        if (invocation is Task task)
        {
            await task.ConfigureAwait(false);
            return task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance)?.GetValue(task);
        }

        Type invocationType = invocation.GetType();
        if (invocationType.FullName?.StartsWith("System.Threading.Tasks.ValueTask", StringComparison.Ordinal) == true)
        {
            MethodInfo? asTask = invocationType.GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
            if (asTask != null)
            {
                Task asyncTask = (Task)asTask.Invoke(invocation, null)!;
                await asyncTask.ConfigureAwait(false);
                return asyncTask.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance)?.GetValue(asyncTask);
            }
        }

        return invocation;
    }








    private static object CreateArgumentsObject(Type argumentType, IDictionary<string, object?> suppliedArguments)
    {
        object[] dictionaryVariants =
        [
                suppliedArguments,
                suppliedArguments.ToArray(),
                suppliedArguments.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray()
        ];

        foreach (ConstructorInfo constructor in argumentType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            var ctorParameters = constructor.GetParameters();
            if (ctorParameters.Length != 1)
            {
                continue;
            }

            foreach (var variant in dictionaryVariants)
                if (ctorParameters[0].ParameterType.IsInstanceOfType(variant))
                {
                    return constructor.Invoke([variant]);
                }
        }

        var argsInstance = Activator.CreateInstance(argumentType)!;
        MethodInfo? addMethod = argumentType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
        if (addMethod != null)
        {
            foreach (var (key, value) in suppliedArguments) addMethod.Invoke(argsInstance, [key, value]);
        }

        return argsInstance;
    }








    private static object? CreateArgumentValue(ParameterInfo parameter, IDictionary<string, object?>? suppliedArguments)
    {
        if (parameter.ParameterType == typeof(CancellationToken))
        {
            return CancellationToken.None;
        }

        if (parameter.ParameterType.Name.Contains("Arguments", StringComparison.Ordinal))
        {
            return CreateArgumentsObject(parameter.ParameterType, suppliedArguments ?? new Dictionary<string, object?>());
        }

        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue;
        }

        return parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
    }








    private static object FindToolByName(IList tools, string name)
    {
        List<string> availableNames = [];

        foreach (var tool in tools)
        {
            PropertyInfo? nameProperty = tool.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            var toolName = nameProperty?.GetValue(tool)?.ToString();
            if (!string.IsNullOrWhiteSpace(toolName))
            {
                availableNames.Add(toolName);
            }

            if (string.Equals(toolName, name, StringComparison.OrdinalIgnoreCase))
            {
                return tool;
            }
        }

        Assert.Fail($"Tool '{name}' was not registered. Available tools: {string.Join(", ", availableNames)}");
        return null!;
    }








    private static IList GetAiTools()
    {
        Type toolBuilderType = Type.GetType("DataIngestionLib.ToolFunctions.ToolBuilder, DataIngestionLib")!;
        MethodInfo getAiTools = toolBuilderType.GetMethod("GetAiTools", BindingFlags.Public | BindingFlags.Static)!;
        var result = getAiTools.Invoke(null, null)!;

        Assert.IsInstanceOfType<IList>(result);
        return (IList)result;
    }








    [TestMethod]
    public async Task GetAiToolsInvokesCuratedReadOnlyToolsAsync()
    {
        IList tools = GetAiTools();

        var systemInfoTool = FindToolByName(tools, "GetInfo");
        var performanceTool = FindToolByName(tools, "ReadSnapshot");

        var systemInfoResult = await InvokeToolAsync(systemInfoTool, null).ConfigureAwait(false);
        var performanceResult = await InvokeToolAsync(performanceTool, new Dictionary<string, object?> { ["sampleDelayMilliseconds"] = 250 }).ConfigureAwait(false);

        AssertToolResultSucceeded(systemInfoResult, "System info tool invocation failed.");
        AssertToolResultSucceeded(performanceResult, "Performance snapshot tool invocation failed.");
    }








    private static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        var property = TryGetProperty(element, propertyName);
        Assert.IsNotNull(property, $"JSON result did not expose a '{propertyName}' property.");
        return property.Value;
    }








    private static async Task<object?> InvokeToolAsync(object tool, IDictionary<string, object?>? arguments)
    {
        MethodInfo? invokeAsync = tool.GetType().GetMethod("InvokeAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(invokeAsync, $"Tool type '{tool.GetType().FullName}' did not expose InvokeAsync.");

        var parameters = invokeAsync.GetParameters();
        var parameterValues = parameters.Select(parameter => CreateArgumentValue(parameter, arguments)).ToArray();

        var invocation = invokeAsync.Invoke(tool, parameterValues);
        return await AwaitInvocationAsync(invocation).ConfigureAwait(false);
    }








    private static JsonElement? TryGetProperty(JsonElement element, string propertyName)
    {
        foreach (JsonProperty property in element.EnumerateObject())
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }

        return null;
    }
}