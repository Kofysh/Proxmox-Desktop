using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Api;

public interface IApiClient : IDisposable
{
    Task<List<RealmData>>   GetRealmsAsync(CancellationToken ct = default);
    Task<LoginResult>       LoginAsync(string username, string password, string realm, string? otp = null, CancellationToken ct = default);
    Task<LoginResult>       LoginWithTokenAsync(string tokenId, string secret, CancellationToken ct = default);
    Task                    RenewTicketAsync(CancellationToken ct = default);
    Task<List<NodeData>>    GetNodesAsync(CancellationToken ct = default);
    Task<List<MachineData>> GetAllMachinesAsync(CancellationToken ct = default);
    Task<PowerResult>       PowerActionAsync(MachineData machine, string action, bool hibernate = false, CancellationToken ct = default);
    Task<string?>           GetConsoleUrlAsync(MachineData machine, string consoleType, CancellationToken ct = default);
    Task<SpiceObject?>      GetSpiceConfigAsync(MachineData machine, string? proxy = null, CancellationToken ct = default);
}
