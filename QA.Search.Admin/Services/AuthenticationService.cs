using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QA.Search.Admin.Errors;
using QA.Search.Data;
using QA.Search.Data.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services
{
    public class AuthenticationService
    {
        private readonly AdminSearchDbContext _dbContext;

        public const string SCHEME = "SearchAdminAuthenticationScheme";
        public const int COOKIE_LIFETIME_DAYS = 1; 
        public const string XSRF_TOKEN_HEADER_NAME = "X-XSRF-TOKEN";

        public AuthenticationService(AdminSearchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Login(HttpContext httpContext, string email, string password)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Equals(email.ToLower()));

            if (user == null || !user.ValidatePassword(password))
            {
                throw new BusinessError("Неверный логин или пароль");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), user.Role))
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, SCHEME));
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(COOKIE_LIFETIME_DAYS)
            };

            await httpContext.SignInAsync(SCHEME,
                claimsPrincipal,
                authProps);

            httpContext.User = claimsPrincipal;
        }

        public async Task Logout(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(SCHEME);

            // SignOutAsync не удаляет информацию о пользователе из контекста
            // Делаем это руками для последующего корректного обновления XSRF-токена в middleware
            httpContext.User = null;
        }
    }
}
