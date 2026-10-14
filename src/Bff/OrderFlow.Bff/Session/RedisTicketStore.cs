using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

namespace OrderFlow.Bff.Session;

internal sealed class RedisTicketStore(
    IDistributedCache cache,
    IDataProtectionProvider dataProtection,
    TimeProvider timeProvider
) : ITicketStore
{
    private const string TicketPrefix = "bff:session:";
    private const string SessionIdPrefix = "bff:sid:";
    private static readonly TimeSpan FallbackLifetime = TimeSpan.FromHours(8);

    private readonly IDataProtector _protector = dataProtection.CreateProtector(
        "OrderFlow.Bff.SessionTicket"
    );

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.CreateVersion7(timeProvider.GetUtcNow()).ToString("N");
        await RenewAsync(key, ticket).ConfigureAwait(false);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresAt =
            ticket.Properties.ExpiresUtc ?? timeProvider.GetUtcNow().Add(FallbackLifetime);
        var entryOptions = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };

        var payload = _protector.Protect(TicketSerializer.Default.Serialize(ticket));
        await cache.SetAsync(TicketPrefix + key, payload, entryOptions).ConfigureAwait(false);

        if (ticket.Principal.FindFirst("sid")?.Value is { } sessionId)
        {
            await cache
                .SetStringAsync(SessionIdPrefix + sessionId, key, entryOptions)
                .ConfigureAwait(false);
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var payload = await cache.GetAsync(TicketPrefix + key).ConfigureAwait(false);
        if (payload is null)
        {
            return null;
        }

        try
        {
            return TicketSerializer.Default.Deserialize(_protector.Unprotect(payload));
        }
        catch (CryptographicException)
        {
            /** Keys rotated or entry tampered with: treat as signed out. */
            return null;
        }
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(TicketPrefix + key);

    public async Task<bool> RemoveBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken
    )
    {
        var key = await cache
            .GetStringAsync(SessionIdPrefix + sessionId, cancellationToken)
            .ConfigureAwait(false);
        if (key is null)
        {
            return false;
        }

        await cache.RemoveAsync(TicketPrefix + key, cancellationToken).ConfigureAwait(false);
        await cache
            .RemoveAsync(SessionIdPrefix + sessionId, cancellationToken)
            .ConfigureAwait(false);
        return true;
    }
}
