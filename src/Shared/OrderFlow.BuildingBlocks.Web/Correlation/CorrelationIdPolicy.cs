namespace OrderFlow.BuildingBlocks.Web.Correlation;

public enum CorrelationIdPolicy
{
    /** Services: reuse the X-Correlation-Id from the gateway; create one only if it is missing or invalid. */
    ReuseIncoming,

    /**
     * Edge hosts (gateway, BFF): the source of correlation ids. A new id is created for every request,
     * except when it arrives from a trusted proxy (TrustedProxy options or loopback), e.g. the BFF calling the gateway.
     */
    Edge,
}
