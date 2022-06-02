using FluentValidation;
using FluentValidation.Results;

namespace Application.Utils
{
    public static class ValidationUtil
    {
        public static void ValidOrThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
