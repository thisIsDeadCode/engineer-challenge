using Grpc.Core;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Queries;
using I_am_engineer.Identity.Transport.Extensions;
using MediatR;

namespace I_am_engineer.Identity.Transport.Grpc.Services;

public class IdentityGrpcService(ISender sender) : IdentityService.IdentityServiceBase
{
    public override async Task<ApiResponseAuthTokens> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new CreateUserCommand(request.Email, request.Password, request.ConfirmPassword), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponseAuthTokens> Login(LoginRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new LoginCommand(request.Email, request.Password), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponseAuthTokens> RefreshSession(RefreshSessionRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new RefreshSessionCommand(request.RefreshToken), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SessionId, out var sessionId))
        {
            return new ApiResponse { IsSuccess = false, Message = "Invalid session id" };
        }

        var response = await sender.Send(new LogoutCommand(sessionId), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponse> RequestPasswordReset(PasswordResetRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new RequestPasswordResetCommand(request.Email), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponse> ConfirmPasswordReset(ConfirmPasswordResetRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }

    public override async Task<ApiResponseMyProfile> GetMyProfile(GetMyProfileRequest request, ServerCallContext context)
    {
        var response = await sender.Send(new GetMyProfileQuery(), context.CancellationToken);
        return response.ToGrpcApiResponse();
    }
}
