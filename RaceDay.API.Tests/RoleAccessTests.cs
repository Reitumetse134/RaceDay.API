using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests
{
    public class RoleAccessTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RoleAccessTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpdateProfile_AsAnyLoggedInUser_ReturnsSuccess()
        {
            var token = await TestHelpers.RegisterAndLoginAsync(
                _client, $"prof{Guid.NewGuid()}@test.com", "Password123!", "Participant");
            TestHelpers.Authorize(_client, token);

            var response = await _client.PutAsJsonAsync("/api/Users", new
            {
                FullName = "Updated",
                LastName = "Name",
                Phone = "0899999999"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetUserById_AsOrganiser_ReturnsSuccessOrNotFound()
        {
            var token = await TestHelpers.RegisterAndLoginAsync(
                _client, $"org3{Guid.NewGuid()}@test.com", "Password123!", "Organiser");
            TestHelpers.Authorize(_client, token);

            var response = await _client.GetAsync("/api/Users/1");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Unexpected status: {response.StatusCode}");
        }
    }
}