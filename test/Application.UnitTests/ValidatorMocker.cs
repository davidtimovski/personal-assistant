﻿using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Application.UnitTests;

internal static class ValidatorMocker
{
    internal static Mock<IValidator<T>> GetSuccessful<T>()
    {
        var validatorMock = new Mock<IValidator<T>>();

        validatorMock.Setup(x => x.Validate(It.IsAny<T>())).Returns(new ValidationResult());

        return validatorMock;
    }

    internal static Mock<IValidator<T>> GetFailed<T>()
    {
        var validatorMock = new Mock<IValidator<T>>();

        validatorMock.Setup(x => x.Validate(It.IsAny<T>()))
            .Returns(new ValidationResult(
                new List<ValidationFailure> { new ValidationFailure("mock", "mock") }));

        return validatorMock;
    }
}
