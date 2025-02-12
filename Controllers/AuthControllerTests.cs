using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Controllers;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock; // 🔹 Mock IUserService
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();

            // ✅ Set the JWT Secret Environment Variable INSIDE the constructor
            Environment.SetEnvironmentVariable("JWT_SECRET", "SuperSecretKeyForTesting12345678901234567890");

            // ✅ Pass the IUserService mock to AuthController
            _authController = new AuthController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Mock ValidateUser to return null (user not found)
            _userServiceMock.Setup(svc => svc.ValidateUser(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var result = await _authController.Login(new LoginDto
            {
                Email = "wrong@example.com",
                Password = "wrongpassword"
            });

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid email or password.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            Console.WriteLine("🔍 Running test: Login_ShouldReturnToken_WhenCredentialsAreValid");

            var user = new User { 
                Id = "123", 
                Email = "valid@example.com", 
                FullName = "Shahar R", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") // Simulate stored hashed password
            };

            _userServiceMock.Setup(svc => svc.ValidateUser("valid@example.com", "password123"))
                .ReturnsAsync(user);

            Console.WriteLine("✅ Mocked ValidateUser to return a valid user.");

            var result = await _authController.Login(new LoginDto
            {
                Email = "valid@example.com",
                Password = "password123"
            });

            Console.WriteLine("📡 Login method executed.");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultData = okResult.Value as dynamic; // Using dynamic for proper access

            Console.WriteLine($"📢 Login response: {resultData}");

            Assert.NotNull(resultData);

            Console.WriteLine("✅ Test Passed: Token generated successfully.");
        }
    }
}
