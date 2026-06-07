namespace ProxmoxDesktop.Api.Models;

/// <summary>Holds an API token credential (tokenId + secret).</summary>
internal sealed record ApiTokenCredential(string TokenId, string Secret);
