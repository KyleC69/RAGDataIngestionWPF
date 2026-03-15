using System.Diagnostics;

using Microsoft.Agents.AI;


namespace DataIngestionLib.Services.ContextInjectors;


public sealed class AIContextHistoryInjector : AIContextProvider
{












    /// <inheritdoc />
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        Debugger.Break();
        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        Debugger.Break();
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        Debugger.Break();
        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        Debugger.Break();
        return base.StoreAIContextAsync(context, cancellationToken);
    }




}
