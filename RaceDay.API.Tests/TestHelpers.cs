using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.API.Tests
{
    public static class TestHelpers
    {
        public static async Task<string> RegisterAndLoginAsync(
            HttpClient client,
            string email,
            string password,
            string role,
            string fullName = "Test",
            string lastName = "User",
            string phone = "0820000000")
        {
            // Register
            var regResponse = await client.PostAsJsonAsync("/api/Auth/register", new
            {
                FullName = fullName,
                LastName = lastName,
                Email = email,
                Password = password,
                Role = role,
                Phone = phone
            });

            if (!regResponse.IsSuccessStatusCode)
            {
                var body = await regResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Register failed for {email} ({role}). Status: {regResponse.StatusCode}. Body: {body}");
            }

            // Login
            var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!loginResponse.IsSuccessStatusCode)
            {
                var body = await loginResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Login failed for {email}. Status: {loginResponse.StatusCode}. Body: {body}");
            }

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();

            if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.Token))
            {
                throw new InvalidOperationException($"Login for {email} succeeded but no token was returned.");
            }

            return loginResult.Token;
        }

        public static void Authorize(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public int UserId { get; set; }
        }
    }
}