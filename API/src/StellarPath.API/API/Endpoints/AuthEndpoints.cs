using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints
{
    public static class AuthEndpoints
    {
        public static WebApplication RegisterAuthEndpoints(this WebApplication app)
        {
            var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication");

            authGroup.MapPost("/google", AuthenticateWithGoogle)
                .WithName("AuthenticateWithGoogle")
                .Produces<AuthResponseDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status500InternalServerError)
                .AllowAnonymous();

            return app;
        }

        private static async Task<IResult> AuthenticateWithGoogle(
           [FromBody] GoogleAuthRequestDto request,
           IGoogleAuthService googleAuthService,
           IUserService userService,
           IJwtService jwtService)
        {
            var payload = await googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            if (payload == null)
            {
                return Results.Unauthorized();
            }

            var userExists = await userService.UserExistsAsync(payload.Subject);
            if (!userExists)
            {
                string firstName = payload.GivenName;
                string lastName = payload.FamilyName ?? "";

                if (string.IsNullOrEmpty(firstName))
                {
                    var userInfo = await googleAuthService.GetGoogleUserInfoAsync(request.AuthToken);
                    if (userInfo != null)
                    {
                        firstName = userInfo.Given_name;
                        lastName = userInfo.Family_name ?? "";
                    }
                }

                await userService.CreateUserAsync(
                    payload.Subject,
                    payload.Email,
                    firstName,
                    lastName);
            }

            var user = await userService.GetUserByGoogleIdAsync(payload.Subject);
            if (user == null || !user.IsActive)
            {
                return Results.Unauthorized();
            }

            var token = jwtService.GenerateToken(user, request.IdToken);

            return Results.Ok(new AuthResponseDto
            {
                Token = token,
                User = user
            });
        }
    }
}
