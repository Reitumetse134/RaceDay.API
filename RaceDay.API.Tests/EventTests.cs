using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EventTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EventTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            var token = await TestHelpers.RegisterAndLoginAsync(
                _client, $"organiser{Guid.NewGuid()}@test.com", "Password123!", "Organiser");
            TestHelpers.Authorize(_client, token);

            var response = await _client.PostAsJsonAsync("/api/Events", new
            {
                EventName = "Test Marathon",
                Description = "A test event.",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Cape Town",
                EventType = "running"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            var token = await TestHelpers.RegisterAndLoginAsync(
                _client, $"participant{Guid.NewGuid()}@test.com", "Password123!", "Participant");
            TestHelpers.Authorize(_client, token);

            var response = await _client.PostAsJsonAsync("/api/Events", new
            {
                EventName = "Unauthorized Event",
                Description = "Should be blocked.",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Durban",
                EventType = "running"
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
        {
            // No authorization header
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.PostAsJsonAsync("/api/Events", new
            {
                EventName = "Anonymous Event",
                Description = "No token.",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Joburg",
                EventType = "running"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEvents_IsPublic()
        {
            var response = await _client.GetAsync("/api/Events");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}