﻿using System.Threading.Tasks;
using Api.Controllers.Accountant;
using Microsoft.AspNetCore.Mvc;
using Api.UnitTests.Builders;
using Xunit;

namespace Api.UnitTests.Controllers.Accountant;

public class UpcomingExpensesControllerTests
{
    private readonly UpcomingExpensesController _sut;

    public UpcomingExpensesControllerTests()
    {
        _sut = new UpcomingExpensesController(null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task Create_Returns400_IfBodyMissing()
    {
        var result = await _sut.Create(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
