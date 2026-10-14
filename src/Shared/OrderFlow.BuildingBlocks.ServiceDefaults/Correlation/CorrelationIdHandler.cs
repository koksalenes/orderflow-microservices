namespace OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

internal sealed class CorrelationIdHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (
            CorrelationContext.Current is { } correlationId
            && !request.Headers.Contains(CorrelationId.HeaderName)
        )
        {
            request.Headers.Add(CorrelationId.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
