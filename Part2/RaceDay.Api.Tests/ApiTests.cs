using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests;

// starts the real API in memory, but swaps SQL Server for a throwaway in-memory database
public class RaceDayFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var old = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>) ||
                d.ServiceType.Name.Contains("IDbContextOptionsConfiguration")).ToList();
            foreach (var d in old)
                services.Remove(d);

            services.AddDbContext<RaceDayDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

public class ApiTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public ApiTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    // ---------- helpers ----------

    private static string NewEmail() => Guid.NewGuid().ToString("N") + "@test.com";

    // each client has its own cookie jar, so each one is its own logged-in session
    private async Task<HttpClient> LoggedInClient(string role)
    {
        var client = _factory.CreateClient();
        string email = NewEmail();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Test User", Email = email, Password = "Pass1234", Role = role });
        await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = "Pass1234" });
        return client;
    }

    private static EventRequest NewEvent() => new EventRequest
    {
        EventName = "Test Run",
        Description = "Test event",
        EventDate = new DateTime(2026, 12, 1),
        Location = "Johannesburg",
        Distance = 10,
        EventType = "Run"
    };

    private static async Task<int> ReadId(HttpResponseMessage response, string name)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var p in json.EnumerateObject())
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                return p.Value.GetInt32();
        throw new Exception("Property not found: " + name);
    }

    private async Task<(int eventId, int categoryId)> CreateEventWithCategory(HttpClient organiser)
    {
        var ev = await organiser.PostAsJsonAsync("/api/events", NewEvent());
        int eventId = await ReadId(ev, "eventID");
        var cat = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new CategoryRequest { CategoryName = "Open" });
        int categoryId = await ReadId(cat, "categoryID");
        return (eventId, categoryId);
    }

    // ---------- registration and login ----------

    [Fact]
    public async Task Register_ValidRequest_Returns201()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Jinhsi", Email = NewEmail(), Password = "Pass1234", Role = "Organiser" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest { FullName = "Yinlin", Email = NewEmail(), Password = "Pass1234", Role = "Participant" };
        await client.PostAsJsonAsync("/api/auth/register", request);
        var second = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Hacker", Email = NewEmail(), Password = "Pass1234", Role = "Admin" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        var client = _factory.CreateClient();
        string email = NewEmail();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Changli", Email = email, Password = "Pass1234", Role = "Organiser" });
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = "Pass1234" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        string email = NewEmail();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Camellya", Email = email, Password = "Pass1234", Role = "Participant" });
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = "WrongPassword" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    // ---------- protected areas need a session ----------

    [Fact]
    public async Task Profile_WhenNotLoggedIn_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WhenLoggedIn_Returns200()
    {
        var client = await LoggedInClient("Participant");
        var response = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- event management and role enforcement ----------

    [Fact]
    public async Task Organiser_CanCreateEvent_Returns201()
    {
        var organiser = await LoggedInClient("Organiser");
        var response = await organiser.PostAsJsonAsync("/api/events", NewEvent());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_Returns403()
    {
        var participant = await LoggedInClient("Participant");
        var response = await participant.PostAsJsonAsync("/api/events", NewEvent());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NotLoggedIn_CannotCreateEvent_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/events", NewEvent());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anyone_CanViewEvents_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEditAnotherOrganisersEvent_Returns403()
    {
        var owner = await LoggedInClient("Organiser");
        var ev = await owner.PostAsJsonAsync("/api/events", NewEvent());
        int eventId = await ReadId(ev, "eventID");

        var otherOrganiser = await LoggedInClient("Organiser");
        var response = await otherOrganiser.PutAsJsonAsync($"/api/events/{eventId}", NewEvent());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCaptureResult_Returns403()
    {
        var participant = await LoggedInClient("Participant");
        var response = await participant.PostAsJsonAsync("/api/results",
            new ResultRequest { EnrolmentId = 1, FinishTime = "00:50:00", FinishPosition = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- enrolments ----------

    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var organiser = await LoggedInClient("Organiser");
        var (eventId, categoryId) = await CreateEventWithCategory(organiser);

        var participant = await LoggedInClient("Participant");
        var enrol = await participant.PostAsJsonAsync("/api/enrolments",
            new EnrolRequest { EventId = eventId, CategoryId = categoryId });
        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);

        var mine = await participant.GetAsync("/api/enrolments/mine");
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
        string body = await mine.Content.ReadAsStringAsync();
        Assert.Contains("Pending", body);
    }

    [Fact]
    public async Task Participant_CannotEnrolTwice_Returns409()
    {
        var organiser = await LoggedInClient("Organiser");
        var (eventId, categoryId) = await CreateEventWithCategory(organiser);

        var participant = await LoggedInClient("Participant");
        var request = new EnrolRequest { EventId = eventId, CategoryId = categoryId };
        await participant.PostAsJsonAsync("/api/enrolments", request);
        var second = await participant.PostAsJsonAsync("/api/enrolments", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_Returns403()
    {
        var organiser = await LoggedInClient("Organiser");
        var (eventId, categoryId) = await CreateEventWithCategory(organiser);

        var response = await organiser.PostAsJsonAsync("/api/enrolments",
            new EnrolRequest { EventId = eventId, CategoryId = categoryId });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}