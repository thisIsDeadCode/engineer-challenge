using Grpc = I_am_engineer.Identity.Transport.Grpc;
using I_am_engineer.Identity.Application.Responses;
using I_am_engineer.Identity.Transport.Models;

namespace I_am_engineer.Identity.Transport.Extensions;

public static class MediatorResponseExtensions
{
    public static ApiResponse<AuthTokensData> ToApiResponse(this AuthTokensResponse response)
    {
        return response.IsSuccess
            ? ApiResponse<AuthTokensData>.Success(new AuthTokensData(response.AccessToken, response.RefreshToken, response.ExpiresAt, response.RefreshTokenExpiresAt))
            : ApiResponse<AuthTokensData>.Error(null, response.Message ?? string.Empty);
    }

    public static ApiResponse<MyProfileData> ToApiResponse(this MyProfileResponse response)
    {
        return response.IsSuccess
            ? ApiResponse<MyProfileData>.Success(new MyProfileData(response.UserId, response.Email, response.DisplayName))
            : ApiResponse<MyProfileData>.Error(null, response.Message ?? string.Empty);
    }

    public static ApiResponse ToApiResponse(this BaseResponse response)
    {
        return response.IsSuccess
            ? ApiResponse.Success()
            : ApiResponse.Error(response.Message ?? string.Empty);
    }

    public static Grpc.ApiResponseAuthTokens ToGrpcApiResponse(this AuthTokensResponse response)
    {
        if (!response.IsSuccess)
        {
            return new Grpc.ApiResponseAuthTokens
            {
                IsSuccess = false,
                Message = response.Message ?? string.Empty
            };
        }

        return new Grpc.ApiResponseAuthTokens
        {
            IsSuccess = true,
            Data = new Grpc.AuthTokensData
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt.ToString("O"),
                RefreshTokenExpiresAt = response.RefreshTokenExpiresAt.ToString("O")
            }
        };
    }

    public static Grpc.ApiResponseMyProfile ToGrpcApiResponse(this MyProfileResponse response)
    {
        if (!response.IsSuccess)
        {
            return new Grpc.ApiResponseMyProfile
            {
                IsSuccess = false,
                Message = response.Message ?? string.Empty
            };
        }

        return new Grpc.ApiResponseMyProfile
        {
            IsSuccess = true,
            Data = new Grpc.MyProfileData
            {
                UserId = response.UserId.ToString(),
                Email = response.Email,
                DisplayName = response.DisplayName
            }
        };
    }

    public static Grpc.ApiResponse ToGrpcApiResponse(this BaseResponse response)
    {
        return response.IsSuccess
            ? new Grpc.ApiResponse { IsSuccess = true }
            : new Grpc.ApiResponse { IsSuccess = false, Message = response.Message ?? string.Empty };
    }
}
