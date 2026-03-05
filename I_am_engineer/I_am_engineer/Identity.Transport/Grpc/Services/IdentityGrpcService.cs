using Grpc.Core;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Queries;
using MediatR;

namespace I_am_engineer.Identity.Transport.Grpc.Services;

public class IdentityGrpcService(ISender sender) : IdentityService.IdentityServiceBase
{
    public override async Task<AuthTokensResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var command = new CreateUserCommand(request.Email, request.Password);
        var response = await sender.Send(command, context.CancellationToken);

        return ToAuthTokensResponse(response);
    }

    public override async Task<AuthTokensResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceId);
        var response = await sender.Send(command, context.CancellationToken);

        return ToAuthTokensResponse(response);
    }

    public override async Task<AuthTokensResponse> RefreshSession(RefreshSessionRequest request, ServerCallContext context)
    {
        var command = new RefreshSessionCommand(request.RefreshToken);
        var response = await sender.Send(command, context.CancellationToken);

        return ToAuthTokensResponse(response);
    }

    public override async Task<BaseResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SessionId, out var sessionId))
        {
            return new BaseResponse { IsSuccess = false, Message = "Invalid session id" };
        }

        var command = new LogoutCommand(sessionId);
        var response = await sender.Send(command, context.CancellationToken);

        return ToBaseResponse(response.IsSuccess, response.Message);
    }

    public override async Task<BaseResponse> RequestPasswordReset(PasswordResetRequest request, ServerCallContext context)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        var response = await sender.Send(command, context.CancellationToken);

        return ToBaseResponse(response.IsSuccess, response.Message);
    }

    public override async Task<BaseResponse> ConfirmPasswordReset(ConfirmPasswordResetRequest request, ServerCallContext context)
    {
        var command = new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword);
        var response = await sender.Send(command, context.CancellationToken);

        return ToBaseResponse(response.IsSuccess, response.Message);
    }

    public override async Task<MyProfileResponse> GetMyProfile(GetMyProfileRequest request, ServerCallContext context)
    {
        var query = new GetMyProfileQuery();
        var response = await sender.Send(query, context.CancellationToken);

        return ToMyProfileResponse(response);
    }

    private static BaseResponse ToBaseResponse(bool isSuccess, string? message)
    {
        if (isSuccess)
        {
            return new BaseResponse
            {
                IsSuccess = true
            };
        }
        else
        {
            return new BaseResponse
            {
                IsSuccess = false,
                Message = message ?? string.Empty
            };
        }
    }

    private static AuthTokensResponse ToAuthTokensResponse(I_am_engineer.Identity.Application.Responses.AuthTokensResponse response)
    {
        if (response.IsSuccess)
        {
            return new AuthTokensResponse
            {
                IsSuccess = true,
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt.ToString("O")
            };
        }
        else
        {
            return new AuthTokensResponse
            {
                IsSuccess = false,
                Message = response.Message ?? string.Empty
            };
        }
    }

    private static MyProfileResponse ToMyProfileResponse(I_am_engineer.Identity.Application.Responses.MyProfileResponse response)
    {
        if (response.IsSuccess)
        {
            return new MyProfileResponse
            {
                IsSuccess = true,
                UserId = response.UserId.ToString(),
                Email = response.Email,
                DisplayName = response.DisplayName
            };
        }
        else
        {
            return new MyProfileResponse
            {
                IsSuccess = false,
                Message = response.Message ?? string.Empty
            };
        }
    }
}
