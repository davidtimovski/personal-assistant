using Core.Application.Contracts.Models.Sender;

namespace Core.Application.Contracts;

public interface ISenderService
{
    Task EnqueueAsync(ISendable sendable);
}
