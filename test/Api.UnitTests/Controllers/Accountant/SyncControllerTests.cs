using System.Threading.Tasks;
using Api.Controllers.Accountant;
using Api.UnitTests.Builders;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.UnitTests.Controllers.Accountant;

public class SyncControllerTests
{
    private readonly SyncController _sut;

    public SyncControllerTests()
    {
        _sut = new SyncController(null, null, null, null, null, null, null, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task GetChanges_Returns400_IfBodyMissing()
    {
        var result = await _sut.GetChanges(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateEntities_Returns400_IfBodyMissing()
    {
        var result = await _sut.CreateEntities(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
