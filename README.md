```md
# 🧪 UserManagementAPI - Testing Suite

This repository contains **unit tests and integration tests** for the **User Management API**.  
It ensures that authentication, user registration, and login functionalities work correctly.

---

## 🚀 **Test Structure**
The test suite is divided into **three main parts**:

| Test Type  | Location | Purpose |
|------------|---------|---------|
| ✅ **Unit Tests** | `Tests/Services/UserServiceTests.cs` | Tests **user validation, registration, and password handling**. |
| ✅ **Controller Unit Tests** | `Tests/Controllers/AuthControllerTests.cs` | Tests API logic using **mocked services** (without hitting the database). |
| ✅ **Integration Tests** | `Tests/Integration/AuthControllerIntegrationTests.cs` | Tests **end-to-end API behavior** using a real HTTP client and MongoDB instance. |

---

## 🛠️ **Setup & Running the Tests**
### **1️⃣ Prerequisites**
Before running the tests, ensure you have:
- **.NET 9 SDK** installed
- **MongoDB running locally** (`mongodb://localhost:27017`)
- **An environment Variables**

---

### **2️⃣ Environment Variables**
Set up the following environment variables **before running tests**:

```sh
# Set these variables in your system or .env file
MONGO_CONNECTION_STRING=mongodb://localhost:27017
MONGO_DATABASE_NAME=UserManagement_TestDB
JWT_SECRET=SuperSecretKeyForTesting12345678901234567890
ASPNETCORE_ENVIRONMENT=Testing
```
📌 **Tests will use a dedicated MongoDB test database (`UserManagement_TestDB`).**  
📌 **This prevents modifying real production data.**  

---

### **3️⃣ Running the Tests**
#### **🔹 Run All Tests**
```sh
dotnet test
```

#### **🔹 Run Only Unit Tests**
```sh
dotnet test --filter FullyQualifiedName~UserManagementAPI.Tests.Services
```

#### **🔹 Run Only Integration Tests**
```sh
dotnet test --filter FullyQualifiedName~UserManagementAPI.Tests.Integration
```

#### **🔹 Run a Specific Test File**
```sh
dotnet test --filter FullyQualifiedName=UserManagementAPI.Tests.Integration.AuthControllerIntegrationTests
```

---

## 🔎 **Unit Tests (`UserServiceTests.cs`)**
**File:** [`Tests/Services/UserServiceTests.cs`](./Tests/Services/UserServiceTests.cs)  
📌 **Tests business logic for user authentication and registration.**

✅ **Tested Features:**
- **RegisterUser()**
  - Returns `true` when a new user is registered.
  - Returns `false` when the email is already in use.
- **ValidateUser()**
  - Returns the user when credentials are valid.
  - Returns `null` when the password is incorrect.

```csharp
[Fact]
public async Task RegisterUser_ShouldReturnTrue_WhenUserIsNew()
{
    _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
        .ReturnsAsync((User?)null);

    bool result = await _userService.RegisterUser("John Doe", "john@example.com", "password123");

    Assert.True(result);
}
```

---

## 🏗️ **Controller Unit Tests (`AuthControllerTests.cs`)**
**File:** [`Tests/Controllers/AuthControllerTests.cs`](./Tests/Controllers/AuthControllerTests.cs)  
📌 **Tests API logic without hitting the database.**

✅ **Tested Features:**
- **Login API**
  - Returns `Unauthorized` when credentials are incorrect.
  - Returns a JWT token when credentials are valid.

```csharp
[Fact]
public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
{
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
```

---

## 🌐 **Integration Tests (`AuthControllerIntegrationTests.cs`)**
**File:** [`Tests/Integration/AuthControllerIntegrationTests.cs`](./Tests/Integration/AuthControllerIntegrationTests.cs)  
📌 **Tests the actual API using an HTTP client and a real MongoDB instance.**

✅ **Tested Features:**
- **Register API**
  - Returns `200 OK` when a new user is created.
  - Returns `400 Bad Request` when a user already exists.
- **Login API**
  - Returns `JWT Token` when credentials are correct.
  - Returns `Unauthorized` when credentials are incorrect.

📌 **🔄 The test database is automatically cleaned before and after tests run.**
```csharp
public void Dispose()
{
    Console.WriteLine("🗑 Cleaning up test database...");
    _usersCollection.DeleteMany(_ => true); // ✅ Delete all test users after tests run
}
```
---

## 📌 **Final Notes**
- ✅ Tests are isolated using **a dedicated test database (`UserManagement_TestDB`)**.
- ✅ **Unit tests use mocks** (`Moq`) to avoid external dependencies.
- ✅ **Integration tests use a real HTTP client** to simulate actual API calls.

---
