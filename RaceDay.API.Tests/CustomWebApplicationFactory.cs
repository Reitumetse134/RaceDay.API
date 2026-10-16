using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.API.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // 👇 The database name must be created ONCE for the factory, not per request
        private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove ALL existing DbContext-related registrations
                var toRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(RaceDayDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IDbContextOptionsConfiguration"))
                ).ToList();

                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                // ✅ Fixed database name — persists across all requests in this factory
                services.AddDbContext<RaceDayDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        }
    }
}