namespace Api.Models;

public abstract class GatewayRequest
{
    public int UserId { get; set; }
    public string Service { get; set; }
    public string Url { get; set; }
}

public class ClientLog : GatewayRequest
{
    public string Application { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public DateTime Occurred { get; set; }
}
