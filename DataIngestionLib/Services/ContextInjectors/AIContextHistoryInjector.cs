using Microsoft.Agents.AI;


namespace DataIngestionLib.Services.ContextInjectors;


public class AIContextHistoryInjector : AIContextProvider
    {

    /// <inheritdoc />
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
        {
        return base.InvokedCoreAsync(context, cancellationToken);
        }


    }