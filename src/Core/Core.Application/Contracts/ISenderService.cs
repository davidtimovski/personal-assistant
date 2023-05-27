using Core.Application.Contracts.Models.Sender;

namespace Core.Application.Contracts;

public interface ISenderService
{
    void Enqueue(ISendable sendable);
}
