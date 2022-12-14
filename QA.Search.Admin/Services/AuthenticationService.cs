using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using QA.Search.Data.Utils;
using Microsoft.EntityFrameworkCore;
using QA.Search.Admin.Errors;
using QA.Search.Data;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Services
{
    public class AuthenticationService
    {
        private readonly SearchDbContext _dbContext;

        public const string Scheme = "SearchAdminAuthenticationScheme";
        public const int CookieLifetimeDays = 1;

        public AuthenticationService(SearchDbContext dbContext)
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

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(CookieLifetimeDays)
            };

            await httpContext.SignInAsync(Scheme,
                claimsPrincipal,
                authProps);

            httpContext.User = claimsPrincipal;
        }

        public async Task Logout(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(Scheme);

            // SignOutAsync не удаляет информацию о пользователе из контекста
            // Делаем это руками для последующего корректного обновления XSRF-токена в middleware
            httpContext.User = null;
        }
    }
}
