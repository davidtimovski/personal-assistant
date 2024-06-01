namespace Core.Application.Utils;

public static class ArgValidator
{
    public static T NotNull<T>(T? argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument;
    }
}
