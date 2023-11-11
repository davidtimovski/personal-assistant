namespace Core.Application.Utils;

public static class ArgValidator
{
    public static T NotNull<T>(T? argument)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        return argument;
    }
}
