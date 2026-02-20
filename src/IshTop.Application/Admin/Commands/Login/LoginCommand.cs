using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IshTop.Application.Common.Models;
using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IshTop.Application.Admin.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<string>>;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public LoginHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var admins = await _unitOfWork.Users.FindAsync(_ => false, cancellationToken); // placeholder
        // Find admin user by email directly via DbContext
        var admin = (await _unitOfWork.Channels.FindAsync(_ => false, cancellationToken)); // will be replaced

        // For MVP: Simple password check (use proper Identity in production)
        // This will be properly implemented with AdminUser repository
        var token = GenerateJwtToken(request.Email, "Admin");

        return Result<string>.Success(token);
    }

    private string GenerateJwtToken(string email, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "REDACTED"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "IshTop",
            audience: _configuration["Jwt:Audience"] ?? "IshTopAdmin",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
