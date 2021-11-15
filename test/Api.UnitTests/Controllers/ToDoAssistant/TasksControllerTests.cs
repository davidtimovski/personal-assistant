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
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.ToDoAssistant
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock = new Mock<ITaskService>();
        private readonly TasksController _sut;

        public TasksControllerTests()
        {
            _sut = new TasksController(
                _taskServiceMock.Object,
                new Mock<IListService>().Object,
                new Mock<IUserService>().Object,
                new Mock<INotificationService>().Object,
                new Mock<ISenderService>().Object,
                new Mock<IValidator<CreateTask>>().Object,
                new Mock<IValidator<BulkCreate>>().Object,
                new Mock<IValidator<UpdateTask>>().Object,
                new Mock<IStringLocalizer<TasksController>>().Object,
                new Mock<IOptions<Urls>>().Object)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void GetReturns404IfNotFound()
        {
            _taskServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((TaskDto)null);

            var result = _sut.Get(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetForUpdateReturns404IfNotFound()
        {
            _taskServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((TaskForUpdate)null);

            var result = _sut.GetForUpdate(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
