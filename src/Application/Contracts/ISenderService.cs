namespace Application.Contracts;

public interface ISenderService
{
    void Enqueue<T>(T message);
}
