using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EnrolmentTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreated()
        {
            // Organiser creates event + category
            var organiserToken = await TestHelpers.RegisterAndLoginAsync(
                _client, $"org{Guid.NewGuid()}@test.com", "Password123!", "Organiser");
            TestHelpers.Authorize(_client, organiserToken);

            var eventResponse = await _client.PostAsJsonAsync("/api/Events", new
            {
                EventName = "Enrolment Test Event",
                Description = "Test.",
                EventDate = DateTime.UtcNow.AddMonths(2),
                Location = "Pretoria",
                EventType = "walking"
            });
            var eventBody = await eventResponse.Content.ReadAsStringAsync();
            var eventId = ExtractId(eventBody, "eventID");

            var categoryResponse = await _client.PostAsJsonAsync(
                $"/api/events/{eventId}/categories", new
                {
                    CategoryName = "Open",
                    Gender = "mixed",
                    MinAge = 18,
                    MaxAge = 99,
                    Distance = 10.0,
                    EntryFee = 100.00
                });
            var categoryBody = await categoryResponse.Content.ReadAsStringAsync();
            var categoryId = ExtractId(categoryBody, "categoryID");

            // Participant enrols
            var participantToken = await TestHelpers.RegisterAndLoginAsync(
                _client, $"part{Guid.NewGuid()}@test.com", "Password123!", "Participant");
            TestHelpers.Authorize(_client, participantToken);

            var enrolResponse = await _client.PostAsync(
                $"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_ReturnsForbidden()
        {
            var organiserToken = await TestHelpers.RegisterAndLoginAsync(
                _client, $"org2{Guid.NewGuid()}@test.com", "Password123!", "Organiser");
            TestHelpers.Authorize(_client, organiserToken);

            // Organiser tries to enrol (should fail)
            var response = await _client.PostAsync("/api/categories/1/enrolments", null);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static int ExtractId(string json, string key)
        {
            var marker = $"\"{key}\":";
            var idx = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return 0;
            var start = idx + marker.Length;
            var end = json.IndexOfAny(new[] { ',', '}' }, start);
            return int.Parse(json.Substring(start, end - start).Trim());
        }
    }
}