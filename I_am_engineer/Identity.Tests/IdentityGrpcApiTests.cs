using Grpc.Net.Client;
using Identity.Tests.GrpcClient;

namespace Identity.Tests;

public sealed class IdentityGrpcApiTests : IClassFixture<TestApplicationFactory>
{
    static IdentityGrpcApiTests()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    private readonly TestApplicationFactory _factory;

    public IdentityGrpcApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }


    [Fact]
    public void GrpcClient_IsGeneratedInTestProject()
    {
        Assert.Equal("Identity.Tests", typeof(IdentityService.IdentityServiceClient).Assembly.GetName().Name);
    }

    [Fact]
    public async Task CreateUser_ReturnsAuthTokens()
    {
        var client = CreateClient();

        var response = await client.CreateUserAsync(new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "Secret123!",
            ConfirmPassword = "Secret123!"
        });

        Assert.True(response.IsSuccess);
        Assert.Equal("access-token", response.Data.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(response.Data.RefreshTokenExpiresAt));
    }

    [Fact]
    public async Task Login_ReturnsAuthTokens()
    {
        var client = CreateClient();

        var response = await client.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Secret123!",
        });

        Assert.True(response.IsSuccess);
        Assert.Equal("refresh-token", response.Data.RefreshToken);
    }

    [Fact]
    public async Task RefreshSession_ReturnsAuthTokens()
    {
        var client = CreateClient();

        var response = await client.RefreshSessionAsync(new RefreshSessionRequest
        {
            RefreshToken = "refresh-token"
        });

        Assert.True(response.IsSuccess);
        Assert.Equal("next-access-token", response.Data.AccessToken);
    }

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        var client = CreateClient();

        var response = await client.LogoutAsync(new LogoutRequest
        {
            SessionId = "11111111-1111-1111-1111-111111111111"
        });

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task RequestPasswordReset_ReturnsSuccess()
    {
        var client = CreateClient();

        var response = await client.RequestPasswordResetAsync(new PasswordResetRequest
        {
            Email = "test@example.com"
        });

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task ConfirmPasswordReset_ReturnsSuccess()
    {
        var client = CreateClient();

        var response = await client.ConfirmPasswordResetAsync(new ConfirmPasswordResetRequest
        {
            ResetToken = "reset-token",
            NewPassword = "Secret123!"
        });

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task GetMyProfile_ReturnsProfile()
    {
        var client = CreateClient();

        var response = await client.GetMyProfileAsync(new GetMyProfileRequest());

        Assert.True(response.IsSuccess);
        Assert.Equal("test@example.com", response.Data.Email);
        Assert.Equal("Integration User", response.Data.DisplayName);
    }

    private IdentityService.IdentityServiceClient CreateClient()
    {
        var handler = _factory.Server.CreateHandler();
        var channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        return new IdentityService.IdentityServiceClient(channel);
    }
}
