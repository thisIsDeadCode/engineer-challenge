using System.Net;
using System.Net.Http.Json;

namespace Identity.Tests;

public sealed class IdentityRestApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public IdentityRestApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateUser_ReturnsAuthTokens()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/identity/create-user", new
        {
            email = "test@example.com",
            password = "Secret123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokensRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
        Assert.Equal("access-token", payload.Data?.AccessToken);
    }

    [Fact]
    public async Task Login_ReturnsAuthTokens()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/identity/login", new
        {
            email = "test@example.com",
            password = "Secret123!",
            deviceId = "device-1"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokensRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
        Assert.Equal("refresh-token", payload.Data?.RefreshToken);
    }

    [Fact]
    public async Task RefreshSession_ReturnsAuthTokens()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/identity/sessions/refresh", new
        {
            refreshToken = "refresh-token"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokensRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
        Assert.Equal("next-access-token", payload.Data?.AccessToken);
    }

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync("/api/v1/identity/Logout/11111111-1111-1111-1111-111111111111");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BaseRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
    }

    [Fact]
    public async Task RequestPasswordReset_ReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/identity/password-resets", new
        {
            email = "test@example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BaseRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
    }

    [Fact]
    public async Task ConfirmPasswordReset_ReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/identity/password-resets/confirm", new
        {
            resetToken = "reset-token",
            newPassword = "Secret123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BaseRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
    }

    [Fact]
    public async Task GetMyProfile_ReturnsProfile()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/identity/sessions/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<MyProfileRestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsSuccess);
        Assert.Equal("test@example.com", payload.Data?.Email);
    }

    private sealed class BaseRestResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }

    private sealed class AuthTokensRestResponse : BaseRestResponse
    {
        public AuthTokensDataResponse? Data { get; set; }
    }

    private sealed class AuthTokensDataResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }

    private sealed class MyProfileRestResponse : BaseRestResponse
    {
        public MyProfileDataResponse? Data { get; set; }
    }

    private sealed class MyProfileDataResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
