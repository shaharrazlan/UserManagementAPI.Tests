using Moq;
using UserManagementAPI.Services;
using UserManagementAPI.Repositories;
using UserManagementAPI.Models;
using Xunit;
using System.Threading.Tasks;

namespace UserManagementAPI.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnTrue_WhenUserIsNew()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync((User?)null); // No user found

            _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<User>()))
                .ReturnsAsync(true); // Simulate successful user creation

            bool result = await _userService.RegisterUser("John Doe", "john@example.com", "password123");

            Assert.True(result);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnFalse_WhenEmailAlreadyExists()
        {
            var existingUser = new User { Email = "john@example.com", FullName = "David E", PasswordHash = "pass123" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail("john@example.com"))
                .ReturnsAsync(existingUser);

            bool result = await _userService.RegisterUser("John Doe", "john@example.com", "password123");

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateUser_ShouldReturnUser_WhenPasswordMatches()
        {
            var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), FullName = "John Doe" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail("test@example.com"))
                .ReturnsAsync(user);

            var result = await _userService.ValidateUser("test@example.com", "password123");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task ValidateUser_ShouldReturnNull_WhenPasswordDoesNotMatch()
        {
            var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), FullName = "John Doe" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail("test@example.com"))
                .ReturnsAsync(user);

            var result = await _userService.ValidateUser("test@example.com", "wrongpassword");

            Assert.Null(result);
        }
    }
}
