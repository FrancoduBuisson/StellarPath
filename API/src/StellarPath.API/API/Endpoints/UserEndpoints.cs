using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;
using System.Security.Claims;

namespace API.Endpoints;
public static class UserEndpoints
{
    public static WebApplication RegisterUserEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("/api/users")
            .WithTags("Users");

        userGroup.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        userGroup.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        userGroup.MapGet("/{googleId}", GetUserById)
            .WithName("GetUserById")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        userGroup.MapPatch("/{googleId}/deactivate", DeactivateUser)
            .WithName("DeactivateUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        userGroup.MapPatch("/{googleId}/activate", ActivateUser)
            .WithName("ActivateUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        userGroup.MapPatch("/{googleId}/role", UpdateUserRole)
            .WithName("UpdateUserRole")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        return app;
    }

    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal user, IUserService userService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        var userDto = await userService.GetUserByGoogleIdAsync(googleId);
        if (userDto == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(userDto);
    }

    private static async Task<IResult> GetAllUsers(
        [FromQuery] string? name,
        [FromQuery] string? firstName,
        [FromQuery] string? lastName,
        [FromQuery] string? email,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        IUserService userService)
    {
        if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(firstName) ||
            !string.IsNullOrEmpty(lastName) || !string.IsNullOrEmpty(email) ||
            !string.IsNullOrEmpty(role) || isActive.HasValue)
        {
            var users = await userService.SearchUsersAsync(
                name, firstName, lastName, email, role, isActive);
            return Results.Ok(users);
        }

        var allUsers = await userService.GetAllUsersAsync();
        return Results.Ok(allUsers);
    }

    private static async Task<IResult> GetUserById(string googleId, IUserService userService)
    {
        var user = await userService.GetUserByGoogleIdAsync(googleId);
        return user != null ? Results.Ok(user) : Results.NotFound();
    }

    private static async Task<IResult> DeactivateUser(string googleId, ClaimsPrincipal user, IUserService userService)
    {
        var currentUserGoogleId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (googleId == currentUserGoogleId)
        {
            return Results.BadRequest("You cannot deactivate your own account");
        }

        var targetUser = await userService.GetUserByGoogleIdAsync(googleId);
        if (targetUser == null)
        {
            return Results.NotFound();
        }

        await userService.DeactivateUserAsync(googleId);
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateUser(string googleId, IUserService userService)
    {
        var targetUser = await userService.GetUserByGoogleIdAsync(googleId);
        if (targetUser == null)
        {
            return Results.NotFound();
        }

        await userService.ActivateUserAsync(googleId);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateUserRole(
        string googleId,
        [FromBody] UpdateUserRoleDto roleUpdate,
        ClaimsPrincipal user,
        IUserService userService)
    {
        var currentUserGoogleId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (googleId == currentUserGoogleId)
        {
            return Results.BadRequest("You cannot change your own role");
        }

        var targetUser = await userService.GetUserByGoogleIdAsync(googleId);
        if (targetUser == null)
        {
            return Results.NotFound();
        }

        var success = await userService.UpdateUserRoleAsync(googleId, roleUpdate.RoleName);
        return success ? Results.NoContent() : Results.BadRequest("Invalid role name");
    }
}