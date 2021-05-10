using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Services.Common;
using PersonalAssistant.Domain.Entities.Common;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepositoryMock = new Mock<IUsersRepository>();
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly IUserService _sut;

        public UserServiceTests()
        {
            _sut = new UserService(
                new Mock<ICdnService>().Object,
                _usersRepositoryMock.Object,
                _tasksRepositoryMock.Object,
                new Mock<IMapper>().Object);
        }

        [Fact]
        public async Task GetToBeNotifiedOfListChangeReturnsEmptyListIfIsPrivateIsTrue()
        {
            const bool isPrivate = true;

            _usersRepositoryMock.Setup(x => x.GetToBeNotifiedOfListChangeAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User> { new User() });

            var result = await _sut.GetToBeNotifiedOfListChangeAsync(It.IsAny<int>(), It.IsAny<int>(), isPrivate);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetToBeNotifiedOfListChangeReturnsEmptyListIfIsPrivateCheckReturnsTrue()
        {
            const bool isPrivate = true;

            _tasksRepositoryMock.Setup(x => x.IsPrivate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(isPrivate);
            _usersRepositoryMock.Setup(x => x.GetToBeNotifiedOfListChangeAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User> { new User() });

            var result = await _sut.GetToBeNotifiedOfListChangeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

            Assert.Empty(result);
        }
    }
}
