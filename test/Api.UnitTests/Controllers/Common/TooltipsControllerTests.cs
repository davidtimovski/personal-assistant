﻿using Api.Controllers.Common;
using Api.UnitTests.Builders;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.UnitTests.Controllers.Common;

public class TooltipsControllerTests
{
    private readonly TooltipsController _sut;

    public TooltipsControllerTests()
    {
        _sut = new TooltipsController(null, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task ToggleDismissed_Returns400_IfBodyMissing()
    {
        var result = await _sut.ToggleDismissed(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
