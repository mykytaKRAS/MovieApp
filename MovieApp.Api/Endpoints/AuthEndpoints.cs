using Microsoft.AspNetCore.Mvc;
using MovieApp.Api.DTOs;
using MovieApp.Api.Services;
using MovieApp.Api.Data;
namespace MovieApp.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var authGroup = app.MapGroup("/api/auth")
                .WithTags("Authentication");

            authGroup.MapPost("/register", async (
                [FromBody] RegisterDto registerDto,
                IAuthService authService) =>
            {
                var result = await authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    return Results.BadRequest(new { message = "Username already exists" });
                }

                return Results.Created($"/api/auth/register/{result.Username}", result);
            })
            .WithName("Register")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            authGroup.MapPost("/login", async (
                [FromBody] LoginDto loginDto,
                IAuthService authService) =>
            {
                var result = await authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(result);
            })
            .WithName("Login")
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            authGroup.MapPost("/logout", async (
                HttpContext context,
                IAuthService authService) =>
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                if (string.IsNullOrEmpty(token))
                {
                    return Results.Unauthorized();
                }

                var result = await authService.LogoutAsync(token);
                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to logout" });
                }

                return Results.Ok(new { message = "Logged out successfully" });
            })
            .WithName("Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            authGroup.MapGet("/validate", async (
                HttpContext context,
                IAuthService authService) =>
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                if (string.IsNullOrEmpty(token))
                {
                    return Results.Unauthorized();
                }

                var isValid = await authService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new { message = "Token is valid", username = context.User.Identity?.Name });
            })
            .WithName("ValidateToken")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}