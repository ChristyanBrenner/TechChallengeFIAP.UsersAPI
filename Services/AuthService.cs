using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utils;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;
        private readonly JwtSettings _jwt;
        private readonly PasswordHasher<Usuario> _hasher;
        //private readonly IPublishEndpoint _publishEndpoint;
        private readonly SqsService _sqsService;

        public AuthService(AppDbContext ctx, IOptions<JwtSettings> jwtOptions, SqsService sqsService)
        {
            _ctx = ctx;
            _jwt = jwtOptions.Value;
            _hasher = new PasswordHasher<Usuario>();
            _sqsService = sqsService;
        }

        public async Task<Usuario> RegisterAsync(RegistroUsuarioDto dto)
        {
            // validação de e-mail simples + senha forte (pode expandir)
            if (_ctx.Usuario.Any(u => u.Email == dto.Email))
                throw new ApplicationException("Email já cadastrado.");

            if (!IsSenhaStrong(dto.Senha))
                throw new ApplicationException("Senha não atende requisitos de segurança.");

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = dto.Senha,
                DataCriacao = DateTime.Now
            };

            usuario.SenhaHash = _hasher.HashPassword(usuario, dto.Senha);
            _ctx.Usuario.Add(usuario);
            await _ctx.SaveChangesAsync();

            var evento = new
            {
                UsuarioId = usuario.Id,
                UsuarioNome = usuario.Nome
            };

            await _sqsService.EnviarUsuarioCriadoAsync(evento);

            return usuario;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var Usuario = _ctx.Usuario.SingleOrDefault(u => u.Email == dto.Email);

            if (Usuario == null) 
                throw new ApplicationException("Credenciais inválidas.");

            var res = _hasher.VerifyHashedPassword(Usuario, Usuario.SenhaHash, dto.Senha);

            if (res == PasswordVerificationResult.Failed) 
                throw new ApplicationException("Credenciais inválidas.");

            return GenerateToken(Usuario);
        }

        private bool IsSenhaStrong(string pwd)
        {
            if (pwd.Length < 8) return false;
            bool hasUpper = pwd.Any(char.IsUpper);
            bool hasLower = pwd.Any(char.IsLower);
            bool hasDigit = pwd.Any(char.IsDigit);
            bool hasSpecial = pwd.Any(ch => !char.IsLetterOrDigit(ch));
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private string GenerateToken(Usuario Usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, Usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, Usuario.Email),
            new Claim(ClaimTypes.Name, Usuario.Nome),
            new Claim(ClaimTypes.Role, Usuario.Role)
        };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
