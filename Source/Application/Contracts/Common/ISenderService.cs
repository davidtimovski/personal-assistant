namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ISenderService
    {
        void Enqueue<T>(T message);
    }
}
