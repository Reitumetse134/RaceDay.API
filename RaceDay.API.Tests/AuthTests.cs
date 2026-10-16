using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            var response = await _client.PostAsJsonAsync("/api/Auth/register", new
            {
                FullName = "Alice",
                LastName = "Smith",
                Email = $"alice{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                Role = "Participant",
                Phone = "0821111111"
            });

            Assert.True(response.IsSuccessStatusCode,
                $"Expected success but got {response.StatusCode}");
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            var email = $"dupe{Guid.NewGuid()}@test.com";
            var dto = new
            {
                FullName = "Bob",
                LastName = "Jones",
                Email = email,
                Password = "Password123!",
                Role = "Participant",
                Phone = "0822222222"
            };

            // First registration succeeds
            await _client.PostAsJsonAsync("/api/Auth/register", dto);

            // Second registration with same email fails
            var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var email = $"login{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/Auth/register", new
            {
                FullName = "Charlie",
                LastName = "Brown",
                Email = email,
                Password = "Password123!",
                Role = "Participant",
                Phone = "0823333333"
            });

            var response = await _client.PostAsJsonAsync("/api/Auth/login", new
            {
                Email = email,
                Password = "Password123!"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var email = $"wrongpw{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/Auth/register", new
            {
                FullName = "Dave",
                LastName = "Doe",
                Email = email,
                Password = "Password123!",
                Role = "Participant",
                Phone = "0824444444"
            });

            var response = await _client.PostAsJsonAsync("/api/Auth/login", new
            {
                Email = email,
                Password = "WrongPassword!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}