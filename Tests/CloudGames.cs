using Domain.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories;
using Services;
using System.Security.Cryptography;
using Utils;
using Xunit;

namespace Tests
{
    public class CloudGames
    {
        public class AuthServiceTests
        {
            [Fact]
            public async Task Register_Should_Create_User_And_Return_Token()
            {
                // Arrange: cria banco em memória isolado
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var ctx = new AppDbContext(options);

                var keyBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(keyBytes);
                var secret = Convert.ToBase64String(keyBytes);

                var jwtSettings = new JwtSettings
                {
                    Key = secret,
                    Issuer = "teste-issuer",
                    Audience = "teste-audience",
                    ExpireMinutes = 60
                };
                IConfiguration config = null;
                var sqsService = new SqsService(config);
                var jwtOptions = Options.Create(jwtSettings);
                var svc = new AuthService(ctx, jwtOptions, sqsService);

                var dto = new RegistroUsuarioDto
                {
                    Nome = "Teste",
                    Email = "teste@ex.com",
                    Senha = "Abcd!234"
                };

                // Act
                var token = await svc.RegisterAsync(dto);

                // Assert
                token.Should();
                ctx.Usuario.Count().Should().Be(1);
                ctx.Usuario.First().Email.Should().Be(dto.Email);
            }
        }
    }
}
