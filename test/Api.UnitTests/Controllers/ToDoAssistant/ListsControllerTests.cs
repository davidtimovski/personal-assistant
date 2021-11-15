using Api.Config;
using Api.Controllers.ToDoAssistant;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using PersonalAssistant.Api.UnitTests.Builders;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.ToDoAssistant
{
    public class ListsControllerTests
    {
        private readonly Mock<IListService> _listServiceMock = new Mock<IListService>();
        private readonly ListsController _sut;

        public ListsControllerTests()
        {
            _sut = new ListsController(
                _listServiceMock.Object,
                new Mock<INotificationService>().Object,
                new Mock<ISenderService>().Object,
                new Mock<IUserService>().Object,
                new Mock<IValidator<CreateList>>().Object,
                new Mock<IValidator<UpdateList>>().Object,
                new Mock<IValidator<UpdateSharedList>>().Object,
                new Mock<IValidator<ShareList>>().Object,
                new Mock<IValidator<CopyList>>().Object,
                new Mock<IStringLocalizer<ListsController>>().Object,
                new Mock<IOptions<Urls>>().Object)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void GetReturns404IfNotFound()
        {
            _listServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EditListDto)null);

            var result = _sut.Get(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetWithSharesReturns404IfNotFound()
        {
            _listServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((ListWithShares)null);

            var result = _sut.GetWithShares(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
