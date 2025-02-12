using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Driver;
using UserManagementAPI;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using Xunit;

namespace UserManagementAPI.Tests.Integration
{
    public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<User> _usersCollection;

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // ✅ Set test environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("MONGO_CONNECTION_STRING", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGO_DATABASE_NAME", "UserManagement_TestDB"); // ✅ Use a dedicated test DB
            Environment.SetEnvironmentVariable("JWT_SECRET", "SuperSecretKeyForTesting12345678901234567890");

            _client = factory.CreateClient();

            // ✅ Connect to the test database
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            _database = mongoClient.GetDatabase("UserManagement_TestDB");
            _usersCollection = _database.GetCollection<User>("Users");

            // ✅ Clear test database before each test
            _usersCollection.DeleteMany(_ => true);
            Console.WriteLine("🗑 Test database cleaned before running tests.");
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenUserIsNew()
        {
            var newUser = new RegisterDto
            {
                FullName = "John Doe",
                Email = "johndoe@example.com",
                PasswordHash = "SecurePass123!" // ✅ Use plain text password, API should hash it
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", newUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📢 Registration API Response: {responseContent}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("User registered successfully", responseContent);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUserAlreadyExists()
        {
            var existingUser = new RegisterDto
            {
                FullName = "Existing User",
                Email = "existing@example.com",
                PasswordHash = "Password123!" // ✅ Use plain text password, API should hash it
            };

            // First registration attempt (should succeed)
            await _client.PostAsJsonAsync("/api/auth/register", existingUser);

            // Second registration attempt (should fail)
            var response = await _client.PostAsJsonAsync("/api/auth/register", existingUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📢 Register Duplicate API Response: {responseContent}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("User already exists", responseContent);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var validUser = new LoginDto
            {
                Email = "valid@example.com",
                Password = "SecurePass123!" // ✅ Use plain text password, API should hash it
            };

            // ✅ Ensure user is registered first
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Valid User",
                Email = validUser.Email,
                PasswordHash = validUser.Password
            });

            // ✅ Attempt to log in
            var response = await _client.PostAsJsonAsync("/api/auth/login", validUser);
            var content = await response.Content.ReadAsStringAsync(); // Read response as a string for debugging

            Console.WriteLine($"📢 Login API Response: {content}");

            // ✅ Check if response is JSON before deserializing
            if (response.Content.Headers.ContentType?.MediaType != "application/json")
            {
                throw new Exception($"Expected JSON but got: {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<dynamic>(); // Now safe to deserialize

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
         
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            var invalidUser = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "wrongpassword"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", invalidUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📢 Invalid Login API Response: {responseContent}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains("Invalid email or password", responseContent);
        }

        // ✅ Cleanup: Delete all users after test run
        public void Dispose()
        {
            Console.WriteLine("🗑 Cleaning up test database...");
            _usersCollection.DeleteMany(_ => true); // ✅ Delete all test users after tests run
        }
    }
}
